using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.AspNetCore.Blazor.Rendering;
using Microsoft.AspNetCore.Blazor.RenderTree;

namespace Microsoft.AspNetCore.Blazor.Server.Rendering
{
    public class PreRenderer : Renderer
    {
        public PreRenderer() : this(new PreServiceProvider())
        {

        }

        public PreRenderer(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public void DoStuff<TComponent>()
        {
            var component = InstantiateComponent(typeof(TComponent));
            var componentId = AssignComponentId(component);
            component.SetParameters(ParameterCollection.Empty);
        }

        protected override void UpdateDisplay(RenderBatch renderBatch)
        {
            var sb = new StringBuilder();

            foreach (var u in renderBatch.UpdatedComponents)
            {
                UpdateComponent(sb, renderBatch, u.ComponentId, u.Edits, renderBatch.ReferenceFrames);

            }

            foreach (var componentID in renderBatch.DisposedComponentIDs)
            {
                DisposeComponent(componentID);
            }

            var s = sb.ToString();
        }

        private void DisposeComponent(int componentId)
        {

        }

        private IList<int> handledComponents = new List<int>();

        private void UpdateComponent(
            StringBuilder sb,
            RenderBatch renderBatch,
            int componentId,
            ArraySegment<RenderTreeEdit> edits,
            ArrayRange<RenderTreeFrame> referenceFrames)
        {
            if (handledComponents.Contains(componentId)) return;
            handledComponents.Add(componentId);
            ApplyEdit(sb, renderBatch, componentId, 0, edits, referenceFrames);
        }

        private void ApplyEdit(
            StringBuilder sb,
            RenderBatch renderBatch,
            int componentId,
            int childIndex,
            ArraySegment<RenderTreeEdit> edits,
            ArrayRange<RenderTreeFrame> referenceFrames)
        {
            //var currentDepth = 0;
            var childIndexAtCurrentDepth = childIndex;
            var maxEditIndexExcl = edits.Offset + edits.Count;

            foreach (var edit in edits)
            {
                switch (edit.Type)
                {
                    case RenderTreeEditType.PrependFrame:
                        var frame = referenceFrames.Array[edit.ReferenceFrameIndex];
                        InsertFrame(
                            sb,
                            renderBatch,
                            componentId,
                            childIndexAtCurrentDepth + edit.SiblingIndex,
                            referenceFrames,
                            frame,
                            edit.ReferenceFrameIndex);
                        break;
                    case RenderTreeEditType.RemoveFrame:
                        break;
                    case RenderTreeEditType.SetAttribute:
                        break;
                    case RenderTreeEditType.RemoveAttribute:
                        break;
                    case RenderTreeEditType.UpdateText:
                        break;
                    case RenderTreeEditType.StepIn:
                        break;
                    case RenderTreeEditType.StepOut:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private int InsertFrame(
            StringBuilder sb,
            RenderBatch renderBatch,
            int componentId,
            int childIndex,
            ArrayRange<RenderTreeFrame> frames,
            RenderTreeFrame frame, int frameIndex
            )
        {
            switch (frame.FrameType)
            {
                case RenderTreeFrameType.Element:
                    InsertElement(sb, renderBatch, componentId, childIndex, frames, frame, frameIndex);
                    return 1;
                case RenderTreeFrameType.Text:
                    InsertText(sb, childIndex, frame);
                    return 1;
                case RenderTreeFrameType.Component:
                    InsertComponent(sb, renderBatch, childIndex, frame, frames);
                    return 1;
                case RenderTreeFrameType.Region:
                    return InsertFrameRange(
                        sb,
                        renderBatch,
                        componentId,
                        childIndex,
                        frames,
                        frameIndex + 1,
                        frameIndex + frame.ElementSubtreeLength);
                case RenderTreeFrameType.Attribute:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void InsertComponent(
            StringBuilder sb,
            RenderBatch renderBatch,
            int childIndex,
            RenderTreeFrame frame,
            ArrayRange<RenderTreeFrame> frames)
        {
            sb.Append("<blazor-component>");
            var u = renderBatch.UpdatedComponents.Single(c => c.ComponentId == frame.ComponentId);
            UpdateComponent(sb, renderBatch, u.ComponentId, u.Edits, frames);
            sb.Append("</blazor-component>");
        }

        private void InsertText(
            StringBuilder sb,
            int childIndex,
            RenderTreeFrame frame)
        {
            var textContent = frame.TextContent;
            sb.Append(textContent);
        }

        private void InsertElement(
            StringBuilder sb,
            RenderBatch renderBatch,
            int componentId,
            int childIndex,
            ArrayRange<RenderTreeFrame> frames,
            RenderTreeFrame frame,
            int frameIndex)
        {
            var tagName = frame.ElementName;
            sb.Append("<");
            sb.Append(tagName);

            var x = frameIndex + frame.ElementSubtreeLength;

            for (var di = frameIndex + 1; di < x; di++)
            {
                var dframe = frames.Array[di];
                if (dframe.FrameType == RenderTreeFrameType.Attribute)
                {
                    ApplyAttribute(sb, componentId, dframe);
                }
                else
                {
                    sb.Append(">");
                    InsertFrameRange(sb, renderBatch, componentId, 0, frames, di, x);
                    sb.Append("</");
                    sb.Append(tagName);
                }
            }

            sb.Append(">");
        }

        private int InsertFrameRange(
            StringBuilder sb,
            RenderBatch renderBatch,
            int componentId,
            int childIndex,
            ArrayRange<RenderTreeFrame> frames,
            int di,
            int i1)
        {
            var org = childIndex;
            for (var i = di; i < i1; i++)
            {
                var frame = frames.Array[i];
                var numAdded = InsertFrame(sb, renderBatch, componentId, childIndex, frames, frame, i);
                childIndex += numAdded;

                var subTreeLength = frame.ElementSubtreeLength;
                if (subTreeLength > 1)
                {
                    i += subTreeLength - 1;
                }
            }

            return (childIndex - org);
        }

        private void ApplyAttribute(StringBuilder sb, int componentId, RenderTreeFrame dframe)
        {
            var attributeName = dframe.AttributeName;
            var value = dframe.AttributeValue;
            sb.Append(" ");
            sb.Append(attributeName);
            sb.Append("=\"");
            sb.Append(value);
            sb.Append("\"");
        }
    }
}
