static void*
mono_wasm_invoke_js_marshalled (MonoString **exceptionMessage, void *asyncHandleLongPtr, MonoString *funcName, MonoString *argsJson)
{
	*exceptionMessage = NULL;
	char *funcNameUtf8 = mono_string_to_utf8 (funcName);
	char *argsJsonUtf8 = argsJson == NULL ? NULL : mono_string_to_utf8 (argsJson);

	MonoString *resultJsonMonoString = (void *)EM_ASM_INT ({
		var mono_string = window._mono_string_cached
			|| (window._mono_string_cached = Module.cwrap('mono_wasm_string_from_js', 'number', ['string']));

		try {
			// Passing a .NET long into JS via Emscripten is tricky. The method here is to pass
			// as pointer to the long, then combine two reads from the HEAPU32 array.
			// Even though JS numbers can't represent the full range of a .NET long, it's OK
			// because we'll never exceed Number.MAX_SAFE_INTEGER (2^53 - 1) in this case.
			var u32Index = $1 >> 2;
			var asyncHandleJsNumber = Module.HEAPU32[u32Index + 1]*4294967296 + Module.HEAPU32[u32Index];

			var funcNameJsString = UTF8ToString ($2);
			var argsJsonJsString = $3 && UTF8ToString ($3);

			var dotNetExports = window.DotNet;
			if (!dotNetExports) {
				throw new Error('The Microsoft.JSInterop.js library is not loaded.');
			}

			if (asyncHandleJsNumber) {
				dotNetExports.jsCallDispatcher.beginInvokeJSFromDotNet(asyncHandleJsNumber, funcNameJsString, argsJsonJsString);
				return 0;
			} else {
				var resultJson = dotNetExports.jsCallDispatcher.invokeJSFromDotNet(funcNameJsString, argsJsonJsString);
				return resultJson === null ? 0 : mono_string(resultJson);
			}
		} catch (ex) {
			var exceptionJsString = ex.message + '\n' + ex.stack;
			var exceptionSystemString = mono_string(exceptionJsString);
			setValue ($0, exceptionSystemString, 'i32'); // *exceptionMessage = exceptionSystemString;
			return 0;
		}
	}, exceptionMessage, (int)asyncHandleLongPtr, (int)funcNameUtf8, (int)argsJsonUtf8);
	
	mono_free (funcNameUtf8);
	if (argsJsonUtf8 != NULL) {
		mono_free (argsJsonUtf8);
	}

	return resultJsonMonoString;
}

static void*
mono_wasm_invoke_js_unmarshalled (MonoString **exceptionMessage, MonoString *funcName, void* arg0, void* arg1, void* arg2)
{
	*exceptionMessage = NULL;
	char *funcNameUtf8 = mono_string_to_utf8 (funcName);
	void *jsCallResult = (void *)EM_ASM_INT ({
		try {
			// Get the function you're trying to invoke
			var funcNameJsString = UTF8ToString ($1);
			var dotNetExports = window.DotNet;
			if (!dotNetExports) {
				throw new Error('The Microsoft.JSInterop.js library is not loaded.');
			}
			var funcInstance = dotNetExports.jsCallDispatcher.findJSFunction(funcNameJsString);

			return funcInstance.call(null, $2, $3, $4);
		} catch (ex) {
			var exceptionJsString = ex.message + '\n' + ex.stack;
			var mono_string = Module.cwrap('mono_wasm_string_from_js', 'number', ['string']); // TODO: Cache
			var exceptionSystemString = mono_string(exceptionJsString);
			setValue ($0, exceptionSystemString, 'i32'); // *exceptionMessage = exceptionSystemString;
			return 0;
		}
	}, exceptionMessage, (int)funcNameUtf8, (int)arg0, (int)arg1, (int)arg2);
	mono_free (funcNameUtf8);

	return jsCallResult;
}
