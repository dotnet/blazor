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

        private StringBuilder _sb;

        public string Render<TComponent>()
        {
            _sb = new StringBuilder();
            var component = InstantiateComponent(typeof(TComponent));
            var componentId = AssignComponentId(component);
            component.SetParameters(ParameterCollection.Empty);
            return _sb.ToString();
        }

        protected override void UpdateDisplay(RenderBatch renderBatch)
        {
            foreach (var u in renderBatch.UpdatedComponents)
            {
                UpdateComponent(renderBatch, u.ComponentId, u.Edits, renderBatch.ReferenceFrames);
            }

            foreach (var componentID in renderBatch.DisposedComponentIDs)
            {
                DisposeComponent(componentID);
            }
        }

        private void DisposeComponent(int componentId)
        {

        }

        private IList<int> handledComponents = new List<int>();

        private void UpdateComponent(
            RenderBatch renderBatch,
            int componentId,
            ArraySegment<RenderTreeEdit> edits,
            ArrayRange<RenderTreeFrame> referenceFrames)
        {
            if (handledComponents.Contains(componentId)) return;
            handledComponents.Add(componentId);
            ApplyEdit(renderBatch, componentId, 0, edits, referenceFrames);
        }

        private void ApplyEdit(
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
                    InsertElement(renderBatch, componentId, childIndex, frames, frame, frameIndex);
                    return 1;
                case RenderTreeFrameType.Text:
                    InsertText(childIndex, frame);
                    return 1;
                case RenderTreeFrameType.Component:
                    InsertComponent(renderBatch, childIndex, frame, frames);
                    return 1;
                case RenderTreeFrameType.Region:
                    return InsertFrameRange(
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
            RenderBatch renderBatch,
            int childIndex,
            RenderTreeFrame frame,
            ArrayRange<RenderTreeFrame> frames)
        {
            _sb.Append("<blazor-component>");
            var u = renderBatch.UpdatedComponents.Single(c => c.ComponentId == frame.ComponentId);
            UpdateComponent(renderBatch, u.ComponentId, u.Edits, frames);
            _sb.Append("</blazor-component>");
        }

        private void InsertText(
            int childIndex,
            RenderTreeFrame frame)
        {
            var textContent = frame.TextContent;
            _sb.Append(textContent);
        }

        private void InsertElement(
            RenderBatch renderBatch,
            int componentId,
            int childIndex,
            ArrayRange<RenderTreeFrame> frames,
            RenderTreeFrame frame,
            int frameIndex)
        {
            var tagName = frame.ElementName;
            _sb.Append("<");
            _sb.Append(tagName);

            var x = frameIndex + frame.ElementSubtreeLength;

            for (var di = frameIndex + 1; di < x; di++)
            {
                var dframe = frames.Array[di];
                if (dframe.FrameType == RenderTreeFrameType.Attribute)
                {
                    ApplyAttribute(componentId, dframe);
                }
                else
                {
                    _sb.Append(">");
                    InsertFrameRange(renderBatch, componentId, 0, frames, di, x);
                    _sb.Append("</");
                    _sb.Append(tagName);
                    break;
                }
            }

            _sb.Append(">");
        }

        private int InsertFrameRange(
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
                var numAdded = InsertFrame(renderBatch, componentId, childIndex, frames, frame, i);
                childIndex += numAdded;

                var subTreeLength = frame.ElementSubtreeLength;
                if (subTreeLength > 1)
                {
                    i += subTreeLength - 1;
                }
            }

            return (childIndex - org);
        }

        private static readonly string[] IgnoredAttributes = {"onclick", "onchange", "onkeypress"};

        private void ApplyAttribute(int componentId, RenderTreeFrame dframe)
        {
            var attributeName = dframe.AttributeName;

            if (IgnoredAttributes.Contains(attributeName))
                return;

            var value = dframe.AttributeValue;
            _sb.Append(" ");
            _sb.Append(attributeName);
            _sb.Append("=\"");
            _sb.Append(value);
            _sb.Append("\"");
        }
    }
}
