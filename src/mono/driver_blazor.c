static void*
blazor_invoke_js (MonoString **exceptionMessage, MonoString *funcName, void* arg0, void* arg1, void* arg2)
{
	*exceptionMessage = NULL;
	char *funcNameUtf8 = mono_string_to_utf8 (funcName);
	void *jsCallResult = (void *)EM_ASM_INT ({
		try {
			// Get the function you're trying to invoke
			var funcNameJsString = UTF8ToString ($1);
			var blazorExports = window.Blazor;
			if (!blazorExports) { // Shouldn't be possible, because you can't start up the .NET code without loading that library
				throw new Error('The Blazor JavaScript library is not loaded.');
			}
			var funcInstance = blazorExports.platform.monoGetRegisteredFunction(funcNameJsString);

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

static void*
blazor_invoke_js_array (MonoString **exceptionMessage, MonoString *funcName, MonoObject *argsArray)
{
	*exceptionMessage = NULL;
	char *funcNameUtf8 = mono_string_to_utf8 (funcName);
	void *jsCallResult = (void *)EM_ASM_INT ({
		try {
			// Get the function you're trying to invoke
			var funcNameJsString = UTF8ToString ($0);
			var blazorExports = window.Blazor;
			if (!blazorExports) { // Shouldn't be possible, because you can't start up the .NET code without loading that library
				throw new Error('The Blazor JavaScript library is not loaded.');
			}
			var funcInstance = blazorExports.platform.monoGetRegisteredFunction(funcNameJsString);
			
			// Map the incoming .NET object array to a JavaScript array of System_Object pointers
			var argsArrayDataPtr = $1 + 12; // First byte from here is length, then following bytes are entries
			var argsArrayLength = Module.getValue(argsArrayDataPtr, 'i32');
			var argsJsArray = [];
			for (var i = 0; i < argsArrayLength; i++) {
				argsArrayDataPtr += 4;
				argsJsArray[i] = Module.getValue(argsArrayDataPtr, 'i32');
			}

			return funcInstance.apply(null, argsJsArray);
		} catch (ex) {
			var exceptionJsString = ex.message + '\n' + ex.stack;
			var mono_string = Module.cwrap('mono_wasm_string_from_js', 'number', ['string']); // TODO: Cache
			var exceptionSystemString = mono_string(exceptionJsString);
			setValue ($2, exceptionSystemString, 'i32'); // *exceptionMessage = exceptionSystemString;
			return 0;
		}
	}, (int)funcNameUtf8, (int)argsArray, exceptionMessage);
	mono_free (funcNameUtf8);

	return jsCallResult;
}
