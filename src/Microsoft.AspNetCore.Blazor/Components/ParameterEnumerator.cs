// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Blazor.RenderTree;
using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Blazor.Components
{
    /// <summary>
    /// An enumerator that iterates through a <see cref="ParameterCollection"/>.
    /// </summary>
    public struct ParameterEnumerator
    {
        private RenderTreeFrameParameterEnumerator _renderTreeParamsEnumerator;
        private TreeParameterEnumerator _treeParameterEnumerator;
        private bool _isEnumeratingRenderTreeParams;

        internal ParameterEnumerator(RenderTreeFrame[] frames, int ownerIndex, IReadOnlyList<TreeParameterState> treeParameters)
        {
            _renderTreeParamsEnumerator = new RenderTreeFrameParameterEnumerator(frames, ownerIndex);
            _treeParameterEnumerator = new TreeParameterEnumerator(treeParameters);
            _isEnumeratingRenderTreeParams = true;
        }

        /// <summary>
        /// Gets the current value of the enumerator.
        /// </summary>
        public Parameter Current => _isEnumeratingRenderTreeParams
            ? _renderTreeParamsEnumerator.Current
            : _treeParameterEnumerator.Current;

        /// <summary>
        /// Instructs the enumerator to move to the next value in the sequence.
        /// </summary>
        /// <returns>A flag to indicate whether or not there is a next value.</returns>
        public bool MoveNext()
        {
            if (_isEnumeratingRenderTreeParams)
            {
                if (_renderTreeParamsEnumerator.MoveNext())
                {
                    return true;
                }
                else
                {
                    _isEnumeratingRenderTreeParams = false;
                }
            }

            return _treeParameterEnumerator.MoveNext();
        }

        struct RenderTreeFrameParameterEnumerator
        {
            private readonly RenderTreeFrame[] _frames;
            private readonly int _ownerIndex;
            private readonly int _ownerDescendantsEndIndexExcl;
            private int _currentIndex;

            internal RenderTreeFrameParameterEnumerator(RenderTreeFrame[] frames, int ownerIndex)
            {
                _frames = frames;
                _ownerIndex = ownerIndex;
                _ownerDescendantsEndIndexExcl = ownerIndex + _frames[ownerIndex].ElementSubtreeLength;
                _currentIndex = ownerIndex;
            }

            
            public Parameter Current
            {
                get
                {
                    if (_currentIndex > _ownerIndex)
                    {
                        ref var frame = ref _frames[_currentIndex];
                        return new Parameter(frame.AttributeName, frame.AttributeValue);
                    }
                    else
                    {
                        throw new InvalidOperationException("Iteration has not yet started.");
                    }
                }
            }

            public bool MoveNext()
            {
                // Stop iteration if you get to the end of the owner's descendants...
                var nextIndex = _currentIndex + 1;
                if (nextIndex == _ownerDescendantsEndIndexExcl)
                {
                    return false;
                }

                // ... or if you get to its first non-attribute descendant (because attributes
                // are always before any other type of descendant)
                if (_frames[nextIndex].FrameType != RenderTreeFrameType.Attribute)
                {
                    return false;
                }

                _currentIndex = nextIndex;
                return true;
            }
        }

        struct TreeParameterEnumerator
        {
            private readonly IReadOnlyList<TreeParameterState> _treeParameters;
            private int _currentIndex;

            public TreeParameterEnumerator(IReadOnlyList<TreeParameterState> treeParameters)
            {
                _treeParameters = treeParameters;
                _currentIndex = -1;
            }

            public Parameter Current => new Parameter(
                _treeParameters[_currentIndex].LocalName,
                _treeParameters[_currentIndex].FromProvider.CurrentValue);

            public bool MoveNext()
            {
                // Bail out early if there are no tree parameters
                if (_treeParameters == null)
                {
                    return false;
                }

                var nextIndex = _currentIndex + 1;
                if (nextIndex < _treeParameters.Count)
                {
                    _currentIndex = nextIndex;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
