// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace BasicTestApp
{
    public partial class CounterComponentWithSeparateCodeFile
    {
        int currentCount = 0;

        // Just to show that we OnInit still works
        protected override void OnInit()
        {
            currentCount = 10;
        }

        void IncrementCount()
        {
            currentCount++;
        }
    }
}
