import { invokeWithJsonMarshalling, invokeWithJsonMarshallingAsync } from './InvokeWithJsonMarshalling';
import { invokeJavaScriptCallback } from './DotNetInvoker';
import { attachRootComponentToElement, renderBatch } from '../Rendering/Renderer';

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
