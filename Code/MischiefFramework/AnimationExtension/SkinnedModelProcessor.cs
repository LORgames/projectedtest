#region File Description
//-----------------------------------------------------------------------------
// SkinnedModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using SkinnedModel;
#endregion

namespace AnimationExtension {
    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class,
    /// adding animation support.
    /// </summary>
    [ContentProcessor(DisplayName="AnimatedModel")]
    public class SkinnedModelProcessor : ModelProcessor {
        /// <summary>
        /// The main Process method converts an intermediate format content pipeline
        /// NodeContent tree to a ModelContent object with embedded animation data.
        /// </summary>
        public override ModelContent Process(NodeContent input,
                                             ContentProcessorContext context) {
            ValidateMesh(input, context, null);

            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            if (skeleton == null)
                throw new InvalidContentException("Input skeleton not found.");

            // We don't want to have to worry about different parts of the model being
            // in different local coordinate systems, so let's just bake everything.
            FlattenTransforms(input, skeleton);

            // Read the bind pose and skeleton hierarchy data.
            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

            if (bones.Count > SkinnedEffect.MaxBones) {
                throw new InvalidContentException(string.Format(
                    "Skeleton has {0} bones, but the maximum supported is {1}.",
                    bones.Count, SkinnedEffect.MaxBones));
            }

            List<Matrix> bindPose = new List<Matrix>();
            List<Matrix> inverseBindPose = new List<Matrix>();
            List<int> skeletonHierarchy = new List<int>();

            foreach (BoneContent bone in bones) {
                bindPose.Add(bone.Transform);
                inverseBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
                skeletonHierarchy.Add(bones.IndexOf(bone.Parent as BoneContent));
            }

            // Convert animation data to our runtime format.
            Dictionary<string, AnimationClip> animationClips;
            animationClips = ProcessAnimations(skeleton.Animations, bones);

            if (animations.Length > 3 && animationClips.Count == 1 && animationClips.ContainsKey("Take 001")) {
                animationClips = SplitAnimations(animationClips["Take 001"], animations);
            }

            // Chain to the base ModelProcessor class so it can convert the model data.
            ModelContent model = base.Process(input, context);

            // Store our custom animation data in the Tag property of the model.
            model.Tag = new SkinningData(animationClips, bindPose,
                                         inverseBindPose, skeletonHierarchy);

            return model;
        }

        private string animations;
        [DisplayName("Animations")]
        [Description("If set, the system will use the animations specified in here, colon delimited animations format: [animatation name first frame last frame] eg: 'walk 1 24;run 27 63'.")]
        [DefaultValue("")]
        public string Animations {
            get { return animations; }
            set { animations = value; }
        }

        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContentDictionary
        /// object to our runtime AnimationClip format.
        /// </summary>
        static Dictionary<string, AnimationClip> ProcessAnimations(
            AnimationContentDictionary animations, IList<BoneContent> bones) {
            // Build up a table mapping bone names to indices.
            Dictionary<string, int> boneMap = new Dictionary<string, int>();

            for (int i = 0; i < bones.Count; i++) {
                string boneName = bones[i].Name;

                if (!string.IsNullOrEmpty(boneName))
                    boneMap.Add(boneName, i);
            }

            // Convert each animation in turn.
            Dictionary<string, AnimationClip> animationClips;
            animationClips = new Dictionary<string, AnimationClip>();

            foreach (KeyValuePair<string, AnimationContent> animation in animations) {
                AnimationClip processed = ProcessAnimation(animation.Value, boneMap);

                animationClips.Add(animation.Key, processed);
            }

            if (animationClips.Count == 0) {
                throw new InvalidContentException(
                            "Input file does not contain any animations.");
            }

            return animationClips;
        }


        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContent
        /// object to our runtime AnimationClip format.
        /// </summary>
        static AnimationClip ProcessAnimation(AnimationContent animation,
                                              Dictionary<string, int> boneMap) {
            List<Keyframe> keyframes = new List<Keyframe>();

            // For each input animation channel.
            foreach (KeyValuePair<string, AnimationChannel> channel in
                animation.Channels) {
                // Look up what bone this channel is controlling.
                int boneIndex;

                if (!boneMap.TryGetValue(channel.Key, out boneIndex)) {
                    throw new InvalidContentException(string.Format(
                        "Found animation for bone '{0}', " +
                        "which is not part of the skeleton.", channel.Key));
                }

                // Convert the keyframe data.
                foreach (AnimationKeyframe keyframe in channel.Value) {
                    keyframes.Add(new Keyframe(boneIndex, keyframe.Time,
                                               keyframe.Transform));
                }
            }

            // Sort the merged keyframes by time.
            keyframes.Sort(CompareKeyframeTimes);

            if (keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            if (animation.Duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            return new AnimationClip(animation.Duration, keyframes);
        }


        /// <summary>
        /// Comparison function for sorting keyframes into ascending time order.
        /// </summary>
        static int CompareKeyframeTimes(Keyframe a, Keyframe b) {
            return a.Time.CompareTo(b.Time);
        }


        /// <summary>
        /// Makes sure this mesh contains the kind of data we know how to animate.
        /// </summary>
        static void ValidateMesh(NodeContent node, ContentProcessorContext context,
                                 string parentBoneName) {
            MeshContent mesh = node as MeshContent;

            if (mesh != null) {
                // Validate the mesh.
                if (parentBoneName != null) {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} is a child of bone {1}. SkinnedModelProcessor " +
                        "does not correctly handle meshes that are children of bones.",
                        mesh.Name, parentBoneName);
                }

                if (!MeshHasSkinning(mesh)) {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} has no skinning information, so it has been deleted.",
                        mesh.Name);

                    mesh.Parent.Children.Remove(mesh);
                    return;
                }
            } else if (node is BoneContent) {
                // If this is a bone, remember that we are now looking inside it.
                parentBoneName = node.Name;
            }

            // Recurse (iterating over a copy of the child collection,
            // because validating children may delete some of them).
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                ValidateMesh(child, context, parentBoneName);
        }


        /// <summary>
        /// Checks whether a mesh contains skininng information.
        /// </summary>
        static bool MeshHasSkinning(MeshContent mesh) {
            foreach (GeometryContent geometry in mesh.Geometry) {
                if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Weights()))
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Bakes unwanted transforms into the model geometry,
        /// so everything ends up in the same coordinate system.
        /// </summary>
        static void FlattenTransforms(NodeContent node, BoneContent skeleton) {
            foreach (NodeContent child in node.Children) {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                // Bake the local transform into the actual geometry.
                MeshHelper.TransformScene(child, child.Transform);

                // Having baked it, we can now set the local
                // coordinate system back to identity.
                child.Transform = Matrix.Identity;

                // Recurse.
                FlattenTransforms(child, skeleton);
            }
        }


        /// <summary>
        /// Force all the materials to use our skinned model effect.
        /// </summary>
        [DefaultValue(MaterialProcessorDefaultEffect.SkinnedEffect)]
        public override MaterialProcessorDefaultEffect DefaultEffect {
            get { return MaterialProcessorDefaultEffect.SkinnedEffect; }
            set { }
        }

        public Dictionary<string, AnimationClip> SplitAnimations(AnimationClip rootAnimation, string animationDefs) {
            Dictionary<string, AnimationClip> splitAnimations = new Dictionary<string, AnimationClip>();
            string[] animationsSplit = animationDefs.Split(';');
            
            foreach(string line in animationsSplit) {
                string[] parts = line.Split(' ');
                string animName = parts[0].Trim('"');
                int startFrame = int.Parse(parts[1]);
                int endFrame = int.Parse(parts[2]);
                splitAnimations[animName] = ExtractAnimation(rootAnimation, startFrame, endFrame);
            }

            return splitAnimations;
        }

        public AnimationClip ExtractAnimation(AnimationClip rootAnimation, int startFrame, int endFrame) {
            TimeSpan startTime = ConvertFrameNumberToTimeSpan(startFrame);
            TimeSpan endTime = ConvertFrameNumberToTimeSpan(endFrame);
            List<Keyframe> keyframes = new List<Keyframe>();
            foreach (Keyframe keyframe in rootAnimation.Keyframes) {
                if (keyframe.Time >= startTime && keyframe.Time <= endTime) {
                    Keyframe newKeyframe = new Keyframe(keyframe.Bone, keyframe.Time - startTime, keyframe.Transform);
                    keyframes.Add(newKeyframe);
                }
            }
            return new AnimationClip(endTime - startTime, keyframes);
        }

        TimeSpan ConvertFrameNumberToTimeSpan(int frameNumber) {
            const float frameTime = 1000f / 24f;
            return new TimeSpan(0, 0, 0, 0, (int)(frameNumber * frameTime));
        }
    }
}
