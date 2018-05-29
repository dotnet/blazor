import { invokeWithJsonMarshalling, invokeWithJsonMarshallingAsync } from './InvokeJavaScriptFunctionWithJsonMarshalling';
import { invokeJavaScriptCallback, InvocationResult } from './InvokeDotNetMethodWithJsonMarshalling';
import { attachRootComponentToElement, renderBatch } from '../Rendering/Renderer';
import { System_String } from '../Platform/Platform';
import { RenderBatchPointer } from '../Rendering/RenderBatch';

/**
 * The definitive list of internal functions invokable from .NET code.
 * These function names are treated as 'reserved' and cannot be passed to registerFunction.
 */
export const internalRegisteredFunctions = {
  attachRootComponentToElement,
  invokeWithJsonMarshalling,
  invokeWithJsonMarshallingAsync,
  invokeJavaScriptCallback,
  renderBatch,
};
