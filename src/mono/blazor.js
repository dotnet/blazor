/******/ (function(modules) { // webpackBootstrap
/******/ 	// The module cache
/******/ 	var installedModules = {};
/******/
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/
/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId]) {
/******/ 			return installedModules[moduleId].exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			i: moduleId,
/******/ 			l: false,
/******/ 			exports: {}
/******/ 		};
/******/
/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/
/******/ 		// Flag the module as loaded
/******/ 		module.l = true;
/******/
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/
/******/
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;
/******/
/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;
/******/
/******/ 	// define getter function for harmony exports
/******/ 	__webpack_require__.d = function(exports, name, getter) {
/******/ 		if(!__webpack_require__.o(exports, name)) {
/******/ 			Object.defineProperty(exports, name, {
/******/ 				configurable: false,
/******/ 				enumerable: true,
/******/ 				get: getter
/******/ 			});
/******/ 		}
/******/ 	};
/******/
/******/ 	// getDefaultExport function for compatibility with non-harmony modules
/******/ 	__webpack_require__.n = function(module) {
/******/ 		var getter = module && module.__esModule ?
/******/ 			function getDefault() { return module['default']; } :
/******/ 			function getModuleExports() { return module; };
/******/ 		__webpack_require__.d(getter, 'a', getter);
/******/ 		return getter;
/******/ 	};
/******/
/******/ 	// Object.prototype.hasOwnProperty.call
/******/ 	__webpack_require__.o = function(object, property) { return Object.prototype.hasOwnProperty.call(object, property); };
/******/
/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = "";
/******/
/******/ 	// Load entry module and return exports
/******/ 	return __webpack_require__(__webpack_require__.s = 4);
/******/ })
/************************************************************************/
/******/ ([
/* 0 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var MonoPlatform_1 = __webpack_require__(5);
exports.platform = MonoPlatform_1.monoPlatform;


/***/ }),
/* 1 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var InternalRegisteredFunction_1 = __webpack_require__(6);
var registeredFunctions = {};
function registerFunction(identifier, implementation) {
    if (InternalRegisteredFunction_1.internalRegisteredFunctions.hasOwnProperty(identifier)) {
        throw new Error("The function identifier '" + identifier + "' is reserved and cannot be registered.");
    }
    if (registeredFunctions.hasOwnProperty(identifier)) {
        throw new Error("A function with the identifier '" + identifier + "' has already been registered.");
    }
    registeredFunctions[identifier] = implementation;
}
exports.registerFunction = registerFunction;
function getRegisteredFunction(identifier) {
    // By prioritising the internal ones, we ensure you can't override them
    var result = InternalRegisteredFunction_1.internalRegisteredFunctions[identifier] || registeredFunctions[identifier];
    if (result) {
        return result;
    }
    else {
        throw new Error("Could not find registered function with name '" + identifier + "'.");
    }
}
exports.getRegisteredFunction = getRegisteredFunction;


/***/ }),
/* 2 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
function getAssemblyNameFromUrl(url) {
    var lastSegment = url.substring(url.lastIndexOf('/') + 1);
    var queryStringStartPos = lastSegment.indexOf('?');
    var filename = queryStringStartPos < 0 ? lastSegment : lastSegment.substring(0, queryStringStartPos);
    return filename.replace(/\.dll$/, '');
}
exports.getAssemblyNameFromUrl = getAssemblyNameFromUrl;


/***/ }),
/* 3 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var Environment_1 = __webpack_require__(0);
var RenderBatch_1 = __webpack_require__(8);
var BrowserRenderer_1 = __webpack_require__(9);
var browserRenderers = {};
function attachComponentToElement(browserRendererId, elementSelector, componentId) {
    var elementSelectorJs = Environment_1.platform.toJavaScriptString(elementSelector);
    var element = document.querySelector(elementSelectorJs);
    if (!element) {
        throw new Error("Could not find any element matching selector '" + elementSelectorJs + "'.");
    }
    var browserRenderer = browserRenderers[browserRendererId];
    if (!browserRenderer) {
        browserRenderer = browserRenderers[browserRendererId] = new BrowserRenderer_1.BrowserRenderer(browserRendererId);
    }
    browserRenderer.attachComponentToElement(componentId, element);
    clearElement(element);
}
exports.attachComponentToElement = attachComponentToElement;
function renderBatch(browserRendererId, batch) {
    var browserRenderer = browserRenderers[browserRendererId];
    if (!browserRenderer) {
        throw new Error("There is no browser renderer with ID " + browserRendererId + ".");
    }
    var updatedComponents = RenderBatch_1.renderBatch.updatedComponents(batch);
    var updatedComponentsLength = RenderBatch_1.arrayRange.count(updatedComponents);
    var updatedComponentsArray = RenderBatch_1.arrayRange.array(updatedComponents);
    var referenceFramesStruct = RenderBatch_1.renderBatch.referenceFrames(batch);
    var referenceFrames = RenderBatch_1.arrayRange.array(referenceFramesStruct);
    for (var i = 0; i < updatedComponentsLength; i++) {
        var diff = Environment_1.platform.getArrayEntryPtr(updatedComponentsArray, i, RenderBatch_1.renderTreeDiffStructLength);
        var componentId = RenderBatch_1.renderTreeDiff.componentId(diff);
        var editsArraySegment = RenderBatch_1.renderTreeDiff.edits(diff);
        var edits = RenderBatch_1.arraySegment.array(editsArraySegment);
        var editsOffset = RenderBatch_1.arraySegment.offset(editsArraySegment);
        var editsLength = RenderBatch_1.arraySegment.count(editsArraySegment);
        browserRenderer.updateComponent(componentId, edits, editsOffset, editsLength, referenceFrames);
    }
    var disposedComponentIds = RenderBatch_1.renderBatch.disposedComponentIds(batch);
    var disposedComponentIdsLength = RenderBatch_1.arrayRange.count(disposedComponentIds);
    var disposedComponentIdsArray = RenderBatch_1.arrayRange.array(disposedComponentIds);
    for (var i = 0; i < disposedComponentIdsLength; i++) {
        var componentIdPtr = Environment_1.platform.getArrayEntryPtr(disposedComponentIdsArray, i, 4);
        var componentId = Environment_1.platform.readInt32Field(componentIdPtr);
        browserRenderer.disposeComponent(componentId);
    }
}
exports.renderBatch = renderBatch;
function clearElement(element) {
    var childNode;
    while (childNode = element.firstChild) {
        element.removeChild(childNode);
    }
}


/***/ }),
/* 4 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = y[op[0] & 2 ? "return" : op[0] ? "throw" : "next"]) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [0, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
var Environment_1 = __webpack_require__(0);
var DotNet_1 = __webpack_require__(2);
__webpack_require__(3);
__webpack_require__(12);
__webpack_require__(13);
__webpack_require__(14);
function boot() {
    return __awaiter(this, void 0, void 0, function () {
        var allScriptElems, thisScriptElem, entryPointDll, entryPointMethod, entryPointAssemblyName, referenceAssembliesCommaSeparated, referenceAssemblies, loadAssemblyUrls, ex_1;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    allScriptElems = document.getElementsByTagName('script');
                    thisScriptElem = (document.currentScript || allScriptElems[allScriptElems.length - 1]);
                    entryPointDll = getRequiredBootScriptAttribute(thisScriptElem, 'main');
                    entryPointMethod = getRequiredBootScriptAttribute(thisScriptElem, 'entrypoint');
                    entryPointAssemblyName = DotNet_1.getAssemblyNameFromUrl(entryPointDll);
                    referenceAssembliesCommaSeparated = thisScriptElem.getAttribute('references') || '';
                    referenceAssemblies = referenceAssembliesCommaSeparated
                        .split(',')
                        .map(function (s) { return s.trim(); })
                        .filter(function (s) { return !!s; });
                    loadAssemblyUrls = [entryPointDll]
                        .concat(referenceAssemblies)
                        .map(function (filename) { return "_framework/_bin/" + filename; });
                    _a.label = 1;
                case 1:
                    _a.trys.push([1, 3, , 4]);
                    return [4 /*yield*/, Environment_1.platform.start(loadAssemblyUrls)];
                case 2:
                    _a.sent();
                    return [3 /*break*/, 4];
                case 3:
                    ex_1 = _a.sent();
                    throw new Error("Failed to start platform. Reason: " + ex_1);
                case 4:
                    // Start up the application
                    Environment_1.platform.callEntryPoint(entryPointAssemblyName, entryPointMethod, []);
                    return [2 /*return*/];
            }
        });
    });
}
function getRequiredBootScriptAttribute(elem, attributeName) {
    var result = elem.getAttribute(attributeName);
    if (!result) {
        throw new Error("Missing \"" + attributeName + "\" attribute on Blazor script tag.");
    }
    return result;
}
boot();


/***/ }),
/* 5 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var DotNet_1 = __webpack_require__(2);
var RegisteredFunction_1 = __webpack_require__(1);
var assembly_load;
var find_class;
var find_method;
var invoke_method;
var mono_string_get_utf8;
var mono_string;
exports.monoPlatform = {
    start: function start(loadAssemblyUrls) {
        return new Promise(function (resolve, reject) {
            // mono.js assumes the existence of this
            window['Browser'] = {
                init: function () { },
                asyncLoad: asyncLoad
            };
            // Emscripten works by expecting the module config to be a global
            window['Module'] = createEmscriptenModuleInstance(loadAssemblyUrls, resolve, reject);
            addScriptTagsToDocument();
        });
    },
    findMethod: function findMethod(assemblyName, namespace, className, methodName) {
        // TODO: Cache the assembly_load outputs?
        var assemblyHandle = assembly_load(assemblyName);
        if (!assemblyHandle) {
            throw new Error("Could not find assembly \"" + assemblyName + "\"");
        }
        var typeHandle = find_class(assemblyHandle, namespace, className);
        if (!typeHandle) {
            throw new Error("Could not find type \"" + className + "\" in namespace \"" + namespace + "\" in assembly \"" + assemblyName + "\"");
        }
        var methodHandle = find_method(typeHandle, methodName, -1);
        if (!methodHandle) {
            throw new Error("Could not find method \"" + methodName + "\" on type \"" + namespace + "." + className + "\"");
        }
        return methodHandle;
    },
    callEntryPoint: function callEntryPoint(assemblyName, entrypointMethod, args) {
        // Parse the entrypointMethod, which is of the form MyApp.MyNamespace.MyTypeName::MyMethodName
        // Note that we don't support specifying a method overload, so it has to be unique
        var entrypointSegments = entrypointMethod.split('::');
        if (entrypointSegments.length != 2) {
            throw new Error('Malformed entry point method name; could not resolve class name and method name.');
        }
        var typeFullName = entrypointSegments[0];
        var methodName = entrypointSegments[1];
        var lastDot = typeFullName.lastIndexOf('.');
        var namespace = lastDot > -1 ? typeFullName.substring(0, lastDot) : '';
        var typeShortName = lastDot > -1 ? typeFullName.substring(lastDot + 1) : typeFullName;
        var entryPointMethodHandle = exports.monoPlatform.findMethod(assemblyName, namespace, typeShortName, methodName);
        exports.monoPlatform.callMethod(entryPointMethodHandle, null, args);
    },
    callMethod: function callMethod(method, target, args) {
        if (args.length > 4) {
            // Hopefully this restriction can be eased soon, but for now make it clear what's going on
            throw new Error("Currently, MonoPlatform supports passing a maximum of 4 arguments from JS to .NET. You tried to pass " + args.length + ".");
        }
        var stack = Module.stackSave();
        try {
            var argsBuffer = Module.stackAlloc(args.length);
            var exceptionFlagManagedInt = Module.stackAlloc(4);
            for (var i = 0; i < args.length; ++i) {
                Module.setValue(argsBuffer + i * 4, args[i], 'i32');
            }
            Module.setValue(exceptionFlagManagedInt, 0, 'i32');
            var res = invoke_method(method, target, argsBuffer, exceptionFlagManagedInt);
            if (Module.getValue(exceptionFlagManagedInt, 'i32') !== 0) {
                // If the exception flag is set, the returned value is exception.ToString()
                throw new Error(exports.monoPlatform.toJavaScriptString(res));
            }
            return res;
        }
        finally {
            Module.stackRestore(stack);
        }
    },
    toJavaScriptString: function toJavaScriptString(managedString) {
        // Comments from original Mono sample:
        //FIXME this is wastefull, we could remove the temp malloc by going the UTF16 route
        //FIXME this is unsafe, cuz raw objects could be GC'd.
        var utf8 = mono_string_get_utf8(managedString);
        var res = Module.UTF8ToString(utf8);
        Module._free(utf8);
        return res;
    },
    toDotNetString: function toDotNetString(jsString) {
        return mono_string(jsString);
    },
    getArrayLength: function getArrayLength(array) {
        return Module.getValue(getArrayDataPointer(array), 'i32');
    },
    getArrayEntryPtr: function getArrayEntryPtr(array, index, itemSize) {
        // First byte is array length, followed by entries
        var address = getArrayDataPointer(array) + 4 + index * itemSize;
        return address;
    },
    getObjectFieldsBaseAddress: function getObjectFieldsBaseAddress(referenceTypedObject) {
        // The first two int32 values are internal Mono data
        return (referenceTypedObject + 8);
    },
    readInt32Field: function readHeapInt32(baseAddress, fieldOffset) {
        return Module.getValue(baseAddress + (fieldOffset || 0), 'i32');
    },
    readObjectField: function readHeapObject(baseAddress, fieldOffset) {
        return Module.getValue(baseAddress + (fieldOffset || 0), 'i32');
    },
    readStringField: function readHeapObject(baseAddress, fieldOffset) {
        var fieldValue = Module.getValue(baseAddress + (fieldOffset || 0), 'i32');
        return fieldValue === 0 ? null : exports.monoPlatform.toJavaScriptString(fieldValue);
    },
    readStructField: function readStructField(baseAddress, fieldOffset) {
        return (baseAddress + (fieldOffset || 0));
    },
};
// Bypass normal type checking to add this extra function. It's only intended to be called from
// the JS code in Mono's driver.c. It's never intended to be called from TypeScript.
exports.monoPlatform.monoGetRegisteredFunction = RegisteredFunction_1.getRegisteredFunction;
function addScriptTagsToDocument() {
    // Load either the wasm or asm.js version of the Mono runtime
    var browserSupportsNativeWebAssembly = typeof WebAssembly !== 'undefined' && WebAssembly.validate;
    var monoRuntimeUrlBase = '_framework/' + (browserSupportsNativeWebAssembly ? 'wasm' : 'asmjs');
    var monoRuntimeScriptUrl = monoRuntimeUrlBase + "/mono.js";
    if (!browserSupportsNativeWebAssembly) {
        // In the asmjs case, the initial memory structure is in a separate file we need to download
        var meminitXHR = Module['memoryInitializerRequest'] = new XMLHttpRequest();
        meminitXHR.open('GET', monoRuntimeUrlBase + "/mono.js.mem");
        meminitXHR.responseType = 'arraybuffer';
        meminitXHR.send(null);
    }
    document.write("<script defer src=\"" + monoRuntimeScriptUrl + "\"></script>");
}
function createEmscriptenModuleInstance(loadAssemblyUrls, onReady, onError) {
    var module = {};
    var wasmBinaryFile = '_framework/wasm/mono.wasm';
    var asmjsCodeFile = '_framework/asmjs/mono.asm.js';
    module.print = function (line) { return console.log("WASM: " + line); };
    module.printErr = function (line) { return console.error("WASM: " + line); };
    module.preRun = [];
    module.postRun = [];
    module.preloadPlugins = [];
    module.locateFile = function (fileName) {
        switch (fileName) {
            case 'mono.wasm': return wasmBinaryFile;
            case 'mono.asm.js': return asmjsCodeFile;
            default: return fileName;
        }
    };
    module.preRun.push(function () {
        // By now, emscripten should be initialised enough that we can capture these methods for later use
        assembly_load = Module.cwrap('mono_wasm_assembly_load', 'number', ['string']);
        find_class = Module.cwrap('mono_wasm_assembly_find_class', 'number', ['number', 'string', 'string']);
        find_method = Module.cwrap('mono_wasm_assembly_find_method', 'number', ['number', 'string', 'number']);
        invoke_method = Module.cwrap('mono_wasm_invoke_method', 'number', ['number', 'number', 'number']);
        mono_string_get_utf8 = Module.cwrap('mono_wasm_string_get_utf8', 'number', ['number']);
        mono_string = Module.cwrap('mono_wasm_string_from_js', 'number', ['string']);
        Module.FS_createPath('/', 'appBinDir', true, true);
        loadAssemblyUrls.forEach(function (url) {
            return FS.createPreloadedFile('appBinDir', DotNet_1.getAssemblyNameFromUrl(url) + ".dll", url, true, false, undefined, onError);
        });
    });
    module.postRun.push(function () {
        var load_runtime = Module.cwrap('mono_wasm_load_runtime', null, ['string']);
        load_runtime('appBinDir');
        onReady();
    });
    return module;
}
function asyncLoad(url, onload, onerror) {
    var xhr = new XMLHttpRequest;
    xhr.open('GET', url, /* async: */ true);
    xhr.responseType = 'arraybuffer';
    xhr.onload = function xhr_onload() {
        if (xhr.status == 200 || xhr.status == 0 && xhr.response) {
            var asm = new Uint8Array(xhr.response);
            onload(asm);
        }
        else {
            onerror(xhr);
        }
    };
    xhr.onerror = onerror;
    xhr.send(null);
}
function getArrayDataPointer(array) {
    return array + 12; // First byte from here is length, then following bytes are entries
}


/***/ }),
/* 6 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var InvokeWithJsonMarshalling_1 = __webpack_require__(7);
var Renderer_1 = __webpack_require__(3);
/**
 * The definitive list of internal functions invokable from .NET code.
 * These function names are treated as 'reserved' and cannot be passed to registerFunction.
 */
exports.internalRegisteredFunctions = {
    attachComponentToElement: Renderer_1.attachComponentToElement,
    invokeWithJsonMarshalling: InvokeWithJsonMarshalling_1.invokeWithJsonMarshalling,
    renderBatch: Renderer_1.renderBatch,
};


/***/ }),
/* 7 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var Environment_1 = __webpack_require__(0);
var RegisteredFunction_1 = __webpack_require__(1);
function invokeWithJsonMarshalling(identifier) {
    var argsJson = [];
    for (var _i = 1; _i < arguments.length; _i++) {
        argsJson[_i - 1] = arguments[_i];
    }
    var identifierJsString = Environment_1.platform.toJavaScriptString(identifier);
    var funcInstance = RegisteredFunction_1.getRegisteredFunction(identifierJsString);
    var args = argsJson.map(function (json) { return JSON.parse(Environment_1.platform.toJavaScriptString(json)); });
    var result = funcInstance.apply(null, args);
    if (result !== null && result !== undefined) {
        var resultJson = JSON.stringify(result);
        return Environment_1.platform.toDotNetString(resultJson);
    }
    else {
        return null;
    }
}
exports.invokeWithJsonMarshalling = invokeWithJsonMarshalling;


/***/ }),
/* 8 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var Environment_1 = __webpack_require__(0);
// Keep in sync with the structs in .NET code
exports.renderBatch = {
    updatedComponents: function (obj) { return Environment_1.platform.readStructField(obj, 0); },
    referenceFrames: function (obj) { return Environment_1.platform.readStructField(obj, arrayRangeStructLength); },
    disposedComponentIds: function (obj) { return Environment_1.platform.readStructField(obj, arrayRangeStructLength + arrayRangeStructLength); },
};
var arrayRangeStructLength = 8;
exports.arrayRange = {
    array: function (obj) { return Environment_1.platform.readObjectField(obj, 0); },
    count: function (obj) { return Environment_1.platform.readInt32Field(obj, 4); },
};
var arraySegmentStructLength = 12;
exports.arraySegment = {
    array: function (obj) { return Environment_1.platform.readObjectField(obj, 0); },
    offset: function (obj) { return Environment_1.platform.readInt32Field(obj, 4); },
    count: function (obj) { return Environment_1.platform.readInt32Field(obj, 8); },
};
exports.renderTreeDiffStructLength = 4 + arraySegmentStructLength;
exports.renderTreeDiff = {
    componentId: function (obj) { return Environment_1.platform.readInt32Field(obj, 0); },
    edits: function (obj) { return Environment_1.platform.readStructField(obj, 4); },
};


/***/ }),
/* 9 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var RenderTreeEdit_1 = __webpack_require__(10);
var RenderTreeFrame_1 = __webpack_require__(11);
var Environment_1 = __webpack_require__(0);
var raiseEventMethod;
var renderComponentMethod;
var BrowserRenderer = /** @class */ (function () {
    function BrowserRenderer(browserRendererId) {
        this.browserRendererId = browserRendererId;
        this.childComponentLocations = {};
    }
    BrowserRenderer.prototype.attachComponentToElement = function (componentId, element) {
        this.childComponentLocations[componentId] = element;
    };
    BrowserRenderer.prototype.updateComponent = function (componentId, edits, editsOffset, editsLength, referenceFrames) {
        var element = this.childComponentLocations[componentId];
        if (!element) {
            throw new Error("No element is currently associated with component " + componentId);
        }
        this.applyEdits(componentId, element, 0, edits, editsOffset, editsLength, referenceFrames);
    };
    BrowserRenderer.prototype.disposeComponent = function (componentId) {
        delete this.childComponentLocations[componentId];
    };
    BrowserRenderer.prototype.applyEdits = function (componentId, parent, childIndex, edits, editsOffset, editsLength, referenceFrames) {
        var currentDepth = 0;
        var childIndexAtCurrentDepth = childIndex;
        var maxEditIndexExcl = editsOffset + editsLength;
        for (var editIndex = editsOffset; editIndex < maxEditIndexExcl; editIndex++) {
            var edit = RenderTreeEdit_1.getRenderTreeEditPtr(edits, editIndex);
            var editType = RenderTreeEdit_1.renderTreeEdit.type(edit);
            switch (editType) {
                case RenderTreeEdit_1.EditType.prependFrame: {
                    var frameIndex = RenderTreeEdit_1.renderTreeEdit.newTreeIndex(edit);
                    var frame = RenderTreeFrame_1.getTreeFramePtr(referenceFrames, frameIndex);
                    var siblingIndex = RenderTreeEdit_1.renderTreeEdit.siblingIndex(edit);
                    this.insertFrame(componentId, parent, childIndexAtCurrentDepth + siblingIndex, referenceFrames, frame, frameIndex);
                    break;
                }
                case RenderTreeEdit_1.EditType.removeFrame: {
                    var siblingIndex = RenderTreeEdit_1.renderTreeEdit.siblingIndex(edit);
                    removeNodeFromDOM(parent, childIndexAtCurrentDepth + siblingIndex);
                    break;
                }
                case RenderTreeEdit_1.EditType.setAttribute: {
                    var frameIndex = RenderTreeEdit_1.renderTreeEdit.newTreeIndex(edit);
                    var frame = RenderTreeFrame_1.getTreeFramePtr(referenceFrames, frameIndex);
                    var siblingIndex = RenderTreeEdit_1.renderTreeEdit.siblingIndex(edit);
                    var element = parent.childNodes[childIndexAtCurrentDepth + siblingIndex];
                    this.applyAttribute(componentId, element, frame);
                    break;
                }
                case RenderTreeEdit_1.EditType.removeAttribute: {
                    var siblingIndex = RenderTreeEdit_1.renderTreeEdit.siblingIndex(edit);
                    removeAttributeFromDOM(parent, childIndexAtCurrentDepth + siblingIndex, RenderTreeEdit_1.renderTreeEdit.removedAttributeName(edit));
                    break;
                }
                case RenderTreeEdit_1.EditType.updateText: {
                    var frameIndex = RenderTreeEdit_1.renderTreeEdit.newTreeIndex(edit);
                    var frame = RenderTreeFrame_1.getTreeFramePtr(referenceFrames, frameIndex);
                    var siblingIndex = RenderTreeEdit_1.renderTreeEdit.siblingIndex(edit);
                    var domTextNode = parent.childNodes[childIndexAtCurrentDepth + siblingIndex];
                    domTextNode.textContent = RenderTreeFrame_1.renderTreeFrame.textContent(frame);
                    break;
                }
                case RenderTreeEdit_1.EditType.stepIn: {
                    var siblingIndex = RenderTreeEdit_1.renderTreeEdit.siblingIndex(edit);
                    parent = parent.childNodes[childIndexAtCurrentDepth + siblingIndex];
                    currentDepth++;
                    childIndexAtCurrentDepth = 0;
                    break;
                }
                case RenderTreeEdit_1.EditType.stepOut: {
                    parent = parent.parentElement;
                    currentDepth--;
                    childIndexAtCurrentDepth = currentDepth === 0 ? childIndex : 0; // The childIndex is only ever nonzero at zero depth
                    break;
                }
                default: {
                    var unknownType = editType; // Compile-time verification that the switch was exhaustive
                    throw new Error("Unknown edit type: " + unknownType);
                }
            }
        }
    };
    BrowserRenderer.prototype.insertFrame = function (componentId, parent, childIndex, frames, frame, frameIndex) {
        var frameType = RenderTreeFrame_1.renderTreeFrame.frameType(frame);
        switch (frameType) {
            case RenderTreeFrame_1.FrameType.element:
                this.insertElement(componentId, parent, childIndex, frames, frame, frameIndex);
                return 1;
            case RenderTreeFrame_1.FrameType.text:
                this.insertText(parent, childIndex, frame);
                return 1;
            case RenderTreeFrame_1.FrameType.attribute:
                throw new Error('Attribute frames should only be present as leading children of element frames.');
            case RenderTreeFrame_1.FrameType.component:
                this.insertComponent(parent, childIndex, frame);
                return 1;
            case RenderTreeFrame_1.FrameType.region:
                return this.insertFrameRange(componentId, parent, childIndex, frames, frameIndex + 1, frameIndex + RenderTreeFrame_1.renderTreeFrame.subtreeLength(frame));
            default:
                var unknownType = frameType; // Compile-time verification that the switch was exhaustive
                throw new Error("Unknown frame type: " + unknownType);
        }
    };
    BrowserRenderer.prototype.insertElement = function (componentId, parent, childIndex, frames, frame, frameIndex) {
        var tagName = RenderTreeFrame_1.renderTreeFrame.elementName(frame);
        var newDomElement = document.createElement(tagName);
        insertNodeIntoDOM(newDomElement, parent, childIndex);
        // Apply attributes
        var descendantsEndIndexExcl = frameIndex + RenderTreeFrame_1.renderTreeFrame.subtreeLength(frame);
        for (var descendantIndex = frameIndex + 1; descendantIndex < descendantsEndIndexExcl; descendantIndex++) {
            var descendantFrame = RenderTreeFrame_1.getTreeFramePtr(frames, descendantIndex);
            if (RenderTreeFrame_1.renderTreeFrame.frameType(descendantFrame) === RenderTreeFrame_1.FrameType.attribute) {
                this.applyAttribute(componentId, newDomElement, descendantFrame);
            }
            else {
                // As soon as we see a non-attribute child, all the subsequent child frames are
                // not attributes, so bail out and insert the remnants recursively
                this.insertFrameRange(componentId, newDomElement, 0, frames, descendantIndex, descendantsEndIndexExcl);
                break;
            }
        }
    };
    BrowserRenderer.prototype.insertComponent = function (parent, childIndex, frame) {
        // Currently, to support O(1) lookups from render tree frames to DOM nodes, we rely on
        // each child component existing as a single top-level element in the DOM. To guarantee
        // that, we wrap child components in these 'blazor-component' wrappers.
        // To improve on this in the future:
        // - If we can statically detect that a given component always produces a single top-level
        //   element anyway, then don't wrap it in a further nonstandard element
        // - If we really want to support child components producing multiple top-level frames and
        //   not being wrapped in a container at all, then every time a component is refreshed in
        //   the DOM, we could update an array on the parent element that specifies how many DOM
        //   nodes correspond to each of its render tree frames. Then when that parent wants to
        //   locate the first DOM node for a render tree frame, it can sum all the frame counts for
        //   all the preceding render trees frames. It's O(N), but where N is the number of siblings
        //   (counting child components as a single item), so N will rarely if ever be large.
        //   We could even keep track of whether all the child components happen to have exactly 1
        //   top level frames, and in that case, there's no need to sum as we can do direct lookups.
        var containerElement = document.createElement('blazor-component');
        insertNodeIntoDOM(containerElement, parent, childIndex);
        // All we have to do is associate the child component ID with its location. We don't actually
        // do any rendering here, because the diff for the child will appear later in the render batch.
        var childComponentId = RenderTreeFrame_1.renderTreeFrame.componentId(frame);
        this.attachComponentToElement(childComponentId, containerElement);
    };
    BrowserRenderer.prototype.insertText = function (parent, childIndex, textFrame) {
        var textContent = RenderTreeFrame_1.renderTreeFrame.textContent(textFrame);
        var newDomTextNode = document.createTextNode(textContent);
        insertNodeIntoDOM(newDomTextNode, parent, childIndex);
    };
    BrowserRenderer.prototype.applyAttribute = function (componentId, toDomElement, attributeFrame) {
        var attributeName = RenderTreeFrame_1.renderTreeFrame.attributeName(attributeFrame);
        var browserRendererId = this.browserRendererId;
        var eventHandlerId = RenderTreeFrame_1.renderTreeFrame.attributeEventHandlerId(attributeFrame);
        if (attributeName === 'value') {
            if (this.tryApplyValueProperty(toDomElement, RenderTreeFrame_1.renderTreeFrame.attributeValue(attributeFrame))) {
                return; // If this DOM element type has special 'value' handling, don't also write it as an attribute
            }
        }
        // TODO: Instead of applying separate event listeners to each DOM element, use event delegation
        // and remove all the _blazor*Listener hacks
        switch (attributeName) {
            case 'onclick': {
                toDomElement.removeEventListener('click', toDomElement['_blazorClickListener']);
                var listener = function (evt) { return raiseEvent(evt, browserRendererId, componentId, eventHandlerId, 'mouse', { Type: 'click' }); };
                toDomElement['_blazorClickListener'] = listener;
                toDomElement.addEventListener('click', listener);
                break;
            }
            case 'onchange': {
                toDomElement.removeEventListener('change', toDomElement['_blazorChangeListener']);
                var targetIsCheckbox_1 = isCheckbox(toDomElement);
                var listener = function (evt) {
                    var newValue = targetIsCheckbox_1 ? evt.target.checked : evt.target.value;
                    raiseEvent(evt, browserRendererId, componentId, eventHandlerId, 'change', { Type: 'change', Value: newValue });
                };
                toDomElement['_blazorChangeListener'] = listener;
                toDomElement.addEventListener('change', listener);
                break;
            }
            case 'onkeypress': {
                toDomElement.removeEventListener('keypress', toDomElement['_blazorKeypressListener']);
                var listener = function (evt) {
                    // This does not account for special keys nor cross-browser differences. So far it's
                    // just to establish that we can pass parameters when raising events.
                    // We use C#-style PascalCase on the eventInfo to simplify deserialization, but this could
                    // change if we introduced a richer JSON library on the .NET side.
                    raiseEvent(evt, browserRendererId, componentId, eventHandlerId, 'keyboard', { Type: evt.type, Key: evt.key });
                };
                toDomElement['_blazorKeypressListener'] = listener;
                toDomElement.addEventListener('keypress', listener);
                break;
            }
            default:
                // Treat as a regular string-valued attribute
                toDomElement.setAttribute(attributeName, RenderTreeFrame_1.renderTreeFrame.attributeValue(attributeFrame));
                break;
        }
    };
    BrowserRenderer.prototype.tryApplyValueProperty = function (element, value) {
        // Certain elements have built-in behaviour for their 'value' property
        switch (element.tagName) {
            case 'INPUT':
            case 'SELECT':
                if (isCheckbox(element)) {
                    element.checked = value === 'True';
                }
                else {
                    // Note: this doen't handle <select> correctly: https://github.com/aspnet/Blazor/issues/157
                    element.value = value;
                }
                return true;
            default:
                return false;
        }
    };
    BrowserRenderer.prototype.insertFrameRange = function (componentId, parent, childIndex, frames, startIndex, endIndexExcl) {
        var origChildIndex = childIndex;
        for (var index = startIndex; index < endIndexExcl; index++) {
            var frame = RenderTreeFrame_1.getTreeFramePtr(frames, index);
            var numChildrenInserted = this.insertFrame(componentId, parent, childIndex, frames, frame, index);
            childIndex += numChildrenInserted;
            // Skip over any descendants, since they are already dealt with recursively
            var subtreeLength = RenderTreeFrame_1.renderTreeFrame.subtreeLength(frame);
            if (subtreeLength > 1) {
                index += subtreeLength - 1;
            }
        }
        return (childIndex - origChildIndex); // Total number of children inserted
    };
    return BrowserRenderer;
}());
exports.BrowserRenderer = BrowserRenderer;
function isCheckbox(element) {
    return element.tagName === 'INPUT' && element.getAttribute('type') === 'checkbox';
}
function insertNodeIntoDOM(node, parent, childIndex) {
    if (childIndex >= parent.childNodes.length) {
        parent.appendChild(node);
    }
    else {
        parent.insertBefore(node, parent.childNodes[childIndex]);
    }
}
function removeNodeFromDOM(parent, childIndex) {
    parent.removeChild(parent.childNodes[childIndex]);
}
function removeAttributeFromDOM(parent, childIndex, attributeName) {
    var element = parent.childNodes[childIndex];
    element.removeAttribute(attributeName);
}
function raiseEvent(event, browserRendererId, componentId, eventHandlerId, eventInfoType, eventInfo) {
    event.preventDefault();
    if (!raiseEventMethod) {
        raiseEventMethod = Environment_1.platform.findMethod('Microsoft.AspNetCore.Blazor.Browser', 'Microsoft.AspNetCore.Blazor.Browser.Rendering', 'BrowserRendererEventDispatcher', 'DispatchEvent');
    }
    var eventDescriptor = {
        BrowserRendererId: browserRendererId,
        ComponentId: componentId,
        EventHandlerId: eventHandlerId,
        EventArgsType: eventInfoType
    };
    Environment_1.platform.callMethod(raiseEventMethod, null, [
        Environment_1.platform.toDotNetString(JSON.stringify(eventDescriptor)),
        Environment_1.platform.toDotNetString(JSON.stringify(eventInfo))
    ]);
}


/***/ }),
/* 10 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var Environment_1 = __webpack_require__(0);
var renderTreeEditStructLength = 16;
function getRenderTreeEditPtr(renderTreeEdits, index) {
    return Environment_1.platform.getArrayEntryPtr(renderTreeEdits, index, renderTreeEditStructLength);
}
exports.getRenderTreeEditPtr = getRenderTreeEditPtr;
exports.renderTreeEdit = {
    // The properties and memory layout must be kept in sync with the .NET equivalent in RenderTreeEdit.cs
    type: function (edit) { return Environment_1.platform.readInt32Field(edit, 0); },
    siblingIndex: function (edit) { return Environment_1.platform.readInt32Field(edit, 4); },
    newTreeIndex: function (edit) { return Environment_1.platform.readInt32Field(edit, 8); },
    removedAttributeName: function (edit) { return Environment_1.platform.readStringField(edit, 12); },
};
var EditType;
(function (EditType) {
    EditType[EditType["prependFrame"] = 1] = "prependFrame";
    EditType[EditType["removeFrame"] = 2] = "removeFrame";
    EditType[EditType["setAttribute"] = 3] = "setAttribute";
    EditType[EditType["removeAttribute"] = 4] = "removeAttribute";
    EditType[EditType["updateText"] = 5] = "updateText";
    EditType[EditType["stepIn"] = 6] = "stepIn";
    EditType[EditType["stepOut"] = 7] = "stepOut";
})(EditType = exports.EditType || (exports.EditType = {}));


/***/ }),
/* 11 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var Environment_1 = __webpack_require__(0);
var renderTreeFrameStructLength = 28;
// To minimise GC pressure, instead of instantiating a JS object to represent each tree frame,
// we work in terms of pointers to the structs on the .NET heap, and use static functions that
// know how to read property values from those structs.
function getTreeFramePtr(renderTreeEntries, index) {
    return Environment_1.platform.getArrayEntryPtr(renderTreeEntries, index, renderTreeFrameStructLength);
}
exports.getTreeFramePtr = getTreeFramePtr;
exports.renderTreeFrame = {
    // The properties and memory layout must be kept in sync with the .NET equivalent in RenderTreeFrame.cs
    frameType: function (frame) { return Environment_1.platform.readInt32Field(frame, 4); },
    subtreeLength: function (frame) { return Environment_1.platform.readInt32Field(frame, 8); },
    componentId: function (frame) { return Environment_1.platform.readInt32Field(frame, 12); },
    elementName: function (frame) { return Environment_1.platform.readStringField(frame, 16); },
    textContent: function (frame) { return Environment_1.platform.readStringField(frame, 16); },
    attributeName: function (frame) { return Environment_1.platform.readStringField(frame, 16); },
    attributeValue: function (frame) { return Environment_1.platform.readStringField(frame, 24); },
    attributeEventHandlerId: function (frame) { return Environment_1.platform.readInt32Field(frame, 8); },
};
var FrameType;
(function (FrameType) {
    // The values must be kept in sync with the .NET equivalent in RenderTreeFrameType.cs
    FrameType[FrameType["element"] = 1] = "element";
    FrameType[FrameType["text"] = 2] = "text";
    FrameType[FrameType["attribute"] = 3] = "attribute";
    FrameType[FrameType["component"] = 4] = "component";
    FrameType[FrameType["region"] = 5] = "region";
})(FrameType = exports.FrameType || (exports.FrameType = {}));


/***/ }),
/* 12 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = y[op[0] & 2 ? "return" : op[0] ? "throw" : "next"]) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [0, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
var RegisteredFunction_1 = __webpack_require__(1);
var Environment_1 = __webpack_require__(0);
var httpClientAssembly = 'Microsoft.AspNetCore.Blazor.Browser';
var httpClientNamespace = httpClientAssembly + ".Http";
var httpClientTypeName = 'BrowserHttpMessageHandler';
var httpClientFullTypeName = httpClientNamespace + "." + httpClientTypeName;
var receiveResponseMethod;
RegisteredFunction_1.registerFunction(httpClientFullTypeName + ".Send", function (id, method, requestUri, body, headersJson) {
    sendAsync(id, method, requestUri, body, headersJson);
});
function sendAsync(id, method, requestUri, body, headersJson) {
    return __awaiter(this, void 0, void 0, function () {
        var response, responseText, ex_1;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    _a.trys.push([0, 3, , 4]);
                    return [4 /*yield*/, fetch(requestUri, {
                            method: method,
                            body: body || undefined,
                            headers: headersJson ? JSON.parse(headersJson) : undefined
                        })];
                case 1:
                    response = _a.sent();
                    return [4 /*yield*/, response.text()];
                case 2:
                    responseText = _a.sent();
                    return [3 /*break*/, 4];
                case 3:
                    ex_1 = _a.sent();
                    dispatchErrorResponse(id, ex_1.toString());
                    return [2 /*return*/];
                case 4:
                    dispatchSuccessResponse(id, response, responseText);
                    return [2 /*return*/];
            }
        });
    });
}
function dispatchSuccessResponse(id, response, responseText) {
    var responseDescriptor = {
        StatusCode: response.status,
        Headers: []
    };
    response.headers.forEach(function (value, name) {
        responseDescriptor.Headers.push([name, value]);
    });
    dispatchResponse(id, Environment_1.platform.toDotNetString(JSON.stringify(responseDescriptor)), Environment_1.platform.toDotNetString(responseText), // TODO: Consider how to handle non-string responses
    /* errorMessage */ null);
}
function dispatchErrorResponse(id, errorMessage) {
    dispatchResponse(id, 
    /* responseDescriptor */ null, 
    /* responseText */ null, Environment_1.platform.toDotNetString(errorMessage));
}
function dispatchResponse(id, responseDescriptor, responseText, errorMessage) {
    if (!receiveResponseMethod) {
        receiveResponseMethod = Environment_1.platform.findMethod(httpClientAssembly, httpClientNamespace, httpClientTypeName, 'ReceiveResponse');
    }
    Environment_1.platform.callMethod(receiveResponseMethod, null, [
        Environment_1.platform.toDotNetString(id.toString()),
        responseDescriptor,
        responseText,
        errorMessage,
    ]);
}


/***/ }),
/* 13 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var RegisteredFunction_1 = __webpack_require__(1);
var Environment_1 = __webpack_require__(0);
var registeredFunctionPrefix = 'Microsoft.AspNetCore.Blazor.Browser.Services.BrowserUriHelper';
var notifyLocationChangedMethod;
var hasRegisteredEventListeners = false;
RegisteredFunction_1.registerFunction(registeredFunctionPrefix + ".getLocationHref", function () { return Environment_1.platform.toDotNetString(location.href); });
RegisteredFunction_1.registerFunction(registeredFunctionPrefix + ".getBaseURI", function () { return document.baseURI ? Environment_1.platform.toDotNetString(document.baseURI) : null; });
RegisteredFunction_1.registerFunction(registeredFunctionPrefix + ".enableNavigationInteception", function () {
    if (hasRegisteredEventListeners) {
        return;
    }
    hasRegisteredEventListeners = true;
    document.addEventListener('click', function (event) {
        // Intercept clicks on all <a> elements where the href is within the <base href> URI space
        var anchorTarget = findClosestAncestor(event.target, 'A');
        if (anchorTarget) {
            var href = anchorTarget.getAttribute('href');
            if (isWithinBaseUriSpace(toAbsoluteUri(href))) {
                event.preventDefault();
                history.pushState(null, /* ignored title */ '', href);
                handleInternalNavigation();
            }
        }
    });
    window.addEventListener('popstate', handleInternalNavigation);
});
function handleInternalNavigation() {
    if (!notifyLocationChangedMethod) {
        notifyLocationChangedMethod = Environment_1.platform.findMethod('Microsoft.AspNetCore.Blazor.Browser', 'Microsoft.AspNetCore.Blazor.Browser.Services', 'BrowserUriHelper', 'NotifyLocationChanged');
    }
    Environment_1.platform.callMethod(notifyLocationChangedMethod, null, [
        Environment_1.platform.toDotNetString(location.href)
    ]);
}
var testAnchor;
function toAbsoluteUri(relativeUri) {
    testAnchor = testAnchor || document.createElement('a');
    testAnchor.href = relativeUri;
    return testAnchor.href;
}
function findClosestAncestor(element, tagName) {
    return !element
        ? null
        : element.tagName === tagName
            ? element
            : findClosestAncestor(element.parentElement, tagName);
}
function isWithinBaseUriSpace(href) {
    var baseUriPrefixWithTrailingSlash = toBaseUriPrefixWithTrailingSlash(document.baseURI); // TODO: Might baseURI really be null?
    return href.startsWith(baseUriPrefixWithTrailingSlash);
}
function toBaseUriPrefixWithTrailingSlash(baseUri) {
    return baseUri.substr(0, baseUri.lastIndexOf('/') + 1);
}


/***/ }),
/* 14 */
/***/ (function(module, exports, __webpack_require__) {

"use strict";

Object.defineProperty(exports, "__esModule", { value: true });
var Environment_1 = __webpack_require__(0);
var RegisteredFunction_1 = __webpack_require__(1);
if (typeof window !== 'undefined') {
    // When the library is loaded in a browser via a <script> element, make the
    // following APIs available in global scope for invocation from JS
    window['Blazor'] = {
        platform: Environment_1.platform,
        registerFunction: RegisteredFunction_1.registerFunction,
    };
}


/***/ })
/******/ ]);
//# sourceMappingURL=data:application/json;charset=utf-8;base64,eyJ2ZXJzaW9uIjozLCJzb3VyY2VzIjpbIndlYnBhY2s6Ly8vd2VicGFjay9ib290c3RyYXAgYWNkY2ViNDQxN2QzYmVlMjcwYjgiLCJ3ZWJwYWNrOi8vLy4vc3JjL0Vudmlyb25tZW50LnRzIiwid2VicGFjazovLy8uL3NyYy9JbnRlcm9wL1JlZ2lzdGVyZWRGdW5jdGlvbi50cyIsIndlYnBhY2s6Ly8vLi9zcmMvUGxhdGZvcm0vRG90TmV0LnRzIiwid2VicGFjazovLy8uL3NyYy9SZW5kZXJpbmcvUmVuZGVyZXIudHMiLCJ3ZWJwYWNrOi8vLy4vc3JjL0Jvb3QudHMiLCJ3ZWJwYWNrOi8vLy4vc3JjL1BsYXRmb3JtL01vbm8vTW9ub1BsYXRmb3JtLnRzIiwid2VicGFjazovLy8uL3NyYy9JbnRlcm9wL0ludGVybmFsUmVnaXN0ZXJlZEZ1bmN0aW9uLnRzIiwid2VicGFjazovLy8uL3NyYy9JbnRlcm9wL0ludm9rZVdpdGhKc29uTWFyc2hhbGxpbmcudHMiLCJ3ZWJwYWNrOi8vLy4vc3JjL1JlbmRlcmluZy9SZW5kZXJCYXRjaC50cyIsIndlYnBhY2s6Ly8vLi9zcmMvUmVuZGVyaW5nL0Jyb3dzZXJSZW5kZXJlci50cyIsIndlYnBhY2s6Ly8vLi9zcmMvUmVuZGVyaW5nL1JlbmRlclRyZWVFZGl0LnRzIiwid2VicGFjazovLy8uL3NyYy9SZW5kZXJpbmcvUmVuZGVyVHJlZUZyYW1lLnRzIiwid2VicGFjazovLy8uL3NyYy9TZXJ2aWNlcy9IdHRwLnRzIiwid2VicGFjazovLy8uL3NyYy9TZXJ2aWNlcy9VcmlIZWxwZXIudHMiLCJ3ZWJwYWNrOi8vLy4vc3JjL0dsb2JhbEV4cG9ydHMudHMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IjtBQUFBO0FBQ0E7O0FBRUE7QUFDQTs7QUFFQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTs7QUFFQTtBQUNBOztBQUVBO0FBQ0E7O0FBRUE7QUFDQTtBQUNBOzs7QUFHQTtBQUNBOztBQUVBO0FBQ0E7O0FBRUE7QUFDQTtBQUNB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LE9BQU8sQ0FBQztBQUMzRCxDQUFDO0FBRUQsOEJBQThCLElBQVk7SUFDeEMsSUFBTSw4QkFBOEIsR0FBRyxnQ0FBZ0MsQ0FBQyxRQUFRLENBQUMsT0FBUSxDQUFDLENBQUMsQ0FBQyxzQ0FBc0M7SUFDbEksTUFBTSxDQUFDLElBQUksQ0FBQyxVQUFVLENBQUMsOEJBQThCLENBQUMsQ0FBQztBQUN6RCxDQUFDO0FBRUQsMENBQTBDLE9BQWU7SUFDdkQsTUFBTSxDQUFDLE9BQU8sQ0FBQyxNQUFNLENBQUMsQ0FBQyxFQUFFLE9BQU8sQ0FBQyxXQUFXLENBQUMsR0FBRyxDQUFDLEdBQUcsQ0FBQyxDQUFDLENBQUM7QUFDekQsQ0FBQzs7Ozs7Ozs7OztBQ3hFRCwyQ0FBd0M7QUFDeEMsa0RBQWdFO0FBRWhFLEVBQUUsQ0FBQyxDQUFDLE9BQU8sTUFBTSxLQUFLLFdBQVcsQ0FBQyxDQUFDLENBQUM7SUFDbEMsMkVBQTJFO0lBQzNFLGtFQUFrRTtJQUNsRSxNQUFNLENBQUMsUUFBUSxDQUFDLEdBQUc7UUFDakIsUUFBUTtRQUNSLGdCQUFnQjtLQUNqQixDQUFDO0FBQ0osQ0FBQyIsImZpbGUiOiJibGF6b3IuanMiLCJzb3VyY2VzQ29udGVudCI6WyIgXHQvLyBUaGUgbW9kdWxlIGNhY2hlXG4gXHR2YXIgaW5zdGFsbGVkTW9kdWxlcyA9IHt9O1xuXG4gXHQvLyBUaGUgcmVxdWlyZSBmdW5jdGlvblxuIFx0ZnVuY3Rpb24gX193ZWJwYWNrX3JlcXVpcmVfXyhtb2R1bGVJZCkge1xuXG4gXHRcdC8vIENoZWNrIGlmIG1vZHVsZSBpcyBpbiBjYWNoZVxuIFx0XHRpZihpbnN0YWxsZWRNb2R1bGVzW21vZHVsZUlkXSkge1xuIFx0XHRcdHJldHVybiBpbnN0YWxsZWRNb2R1bGVzW21vZHVsZUlkXS5leHBvcnRzO1xuIFx0XHR9XG4gXHRcdC8vIENyZWF0ZSBhIG5ldyBtb2R1bGUgKGFuZCBwdXQgaXQgaW50byB0aGUgY2FjaGUpXG4gXHRcdHZhciBtb2R1bGUgPSBpbnN0YWxsZWRNb2R1bGVzW21vZHVsZUlkXSA9IHtcbiBcdFx0XHRpOiBtb2R1bGVJZCxcbiBcdFx0XHRsOiBmYWxzZSxcbiBcdFx0XHRleHBvcnRzOiB7fVxuIFx0XHR9O1xuXG4gXHRcdC8vIEV4ZWN1dGUgdGhlIG1vZHVsZSBmdW5jdGlvblxuIFx0XHRtb2R1bGVzW21vZHVsZUlkXS5jYWxsKG1vZHVsZS5leHBvcnRzLCBtb2R1bGUsIG1vZHVsZS5leHBvcnRzLCBfX3dlYnBhY2tfcmVxdWlyZV9fKTtcblxuIFx0XHQvLyBGbGFnIHRoZSBtb2R1bGUgYXMgbG9hZGVkXG4gXHRcdG1vZHVsZS5sID0gdHJ1ZTtcblxuIFx0XHQvLyBSZXR1cm4gdGhlIGV4cG9ydHMgb2YgdGhlIG1vZHVsZVxuIFx0XHRyZXR1cm4gbW9kdWxlLmV4cG9ydHM7XG4gXHR9XG5cblxuIFx0Ly8gZXhwb3NlIHRoZSBtb2R1bGVzIG9iamVjdCAoX193ZWJwYWNrX21vZHVsZXNfXylcbiBcdF9fd2VicGFja19yZXF1aXJlX18ubSA9IG1vZHVsZXM7XG5cbiBcdC8vIGV4cG9zZSB0aGUgbW9kdWxlIGNhY2hlXG4gXHRfX3dlYnBhY2tfcmVxdWlyZV9fLmMgPSBpbnN0YWxsZWRNb2R1bGVzO1xuXG4gXHQvLyBkZWZpbmUgZ2V0dGVyIGZ1bmN0aW9uIGZvciBoYXJtb255IGV4cG9ydHNcbiBcdF9fd2VicGFja19yZXF1aXJlX18uZCA9IGZ1bmN0aW9uKGV4cG9ydHMsIG5hbWUsIGdldHRlcikge1xuIFx0XHRpZighX193ZWJwYWNrX3JlcXVpcmVfXy5vKGV4cG9ydHMsIG5hbWUpKSB7XG4gXHRcdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KGV4cG9ydHMsIG5hbWUsIHtcbiBcdFx0XHRcdGNvbmZpZ3VyYWJsZTogZmFsc2UsXG4gXHRcdFx0XHRlbnVtZXJhYmxlOiB0cnVlLFxuIFx0XHRcdFx0Z2V0OiBnZXR0ZXJcbiBcdFx0XHR9KTtcbiBcdFx0fVxuIFx0fTtcblxuIFx0Ly8gZ2V0RGVmYXVsdEV4cG9ydCBmdW5jdGlvbiBmb3IgY29tcGF0aWJpbGl0eSB3aXRoIG5vbi1oYXJtb255IG1vZHVsZXNcbiBcdF9fd2VicGFja19yZXF1aXJlX18ubiA9IGZ1bmN0aW9uKG1vZHVsZSkge1xuIFx0XHR2YXIgZ2V0dGVyID0gbW9kdWxlICYmIG1vZHVsZS5fX2VzTW9kdWxlID9cbiBcdFx0XHRmdW5jdGlvbiBnZXREZWZhdWx0KCkgeyByZXR1cm4gbW9kdWxlWydkZWZhdWx0J107IH0gOlxuIFx0XHRcdGZ1bmN0aW9uIGdldE1vZHVsZUV4cG9ydHMoKSB7IHJldHVybiBtb2R1bGU7IH07XG4gXHRcdF9fd2VicGFja19yZXF1aXJlX18uZChnZXR0ZXIsICdhJywgZ2V0dGVyKTtcbiBcdFx0cmV0dXJuIGdldHRlcjtcbiBcdH07XG5cbiBcdC8vIE9iamVjdC5wcm90b3R5cGUuaGFzT3duUHJvcGVydHkuY2FsbFxuIFx0X193ZWJwYWNrX3JlcXVpcmVfXy5vID0gZnVuY3Rpb24ob2JqZWN0LCBwcm9wZXJ0eSkgeyByZXR1cm4gT2JqZWN0LnByb3RvdHlwZS5oYXNPd25Qcm9wZXJ0eS5jYWxsKG9iamVjdCwgcHJvcGVydHkpOyB9O1xuXG4gXHQvLyBfX3dlYnBhY2tfcHVibGljX3BhdGhfX1xuIFx0X193ZWJwYWNrX3JlcXVpcmVfXy5wID0gXCJcIjtcblxuIFx0Ly8gTG9hZCBlbnRyeSBtb2R1bGUgYW5kIHJldHVybiBleHBvcnRzXG4gXHRyZXR1cm4gX193ZWJwYWNrX3JlcXVpcmVfXyhfX3dlYnBhY2tfcmVxdWlyZV9fLnMgPSA0KTtcblxuXG5cbi8vIFdFQlBBQ0sgRk9PVEVSIC8vXG4vLyB3ZWJwYWNrL2Jvb3RzdHJhcCBhY2RjZWI0NDE3ZDNiZWUyNzBiOCIsIi8vIEV4cG9zZSBhbiBleHBvcnQgY2FsbGVkICdwbGF0Zm9ybScgb2YgdGhlIGludGVyZmFjZSB0eXBlICdQbGF0Zm9ybScsXHJcbi8vIHNvIHRoYXQgY29uc3VtZXJzIGNhbiBiZSBhZ25vc3RpYyBhYm91dCB3aGljaCBpbXBsZW1lbnRhdGlvbiB0aGV5IHVzZS5cclxuLy8gQmFzaWMgYWx0ZXJuYXRpdmUgdG8gaGF2aW5nIGFuIGFjdHVhbCBESSBjb250YWluZXIuXHJcbmltcG9ydCB7IFBsYXRmb3JtIH0gZnJvbSAnLi9QbGF0Zm9ybS9QbGF0Zm9ybSc7XHJcbmltcG9ydCB7IG1vbm9QbGF0Zm9ybSB9IGZyb20gJy4vUGxhdGZvcm0vTW9uby9Nb25vUGxhdGZvcm0nO1xyXG5leHBvcnQgY29uc3QgcGxhdGZvcm06IFBsYXRmb3JtID0gbW9ub1BsYXRmb3JtO1xyXG5cblxuXG4vLyBXRUJQQUNLIEZPT1RFUiAvL1xuLy8gLi9zcmMvRW52aXJvbm1lbnQudHMiLCJpbXBvcnQgeyBpbnRlcm5hbFJlZ2lzdGVyZWRGdW5jdGlvbnMgfSBmcm9tICcuL0ludGVybmFsUmVnaXN0ZXJlZEZ1bmN0aW9uJztcclxuXHJcbmNvbnN0IHJlZ2lzdGVyZWRGdW5jdGlvbnM6IHsgW2lkZW50aWZpZXI6IHN0cmluZ106IEZ1bmN0aW9uIHwgdW5kZWZpbmVkIH0gPSB7fTtcclxuXHJcbmV4cG9ydCBmdW5jdGlvbiByZWdpc3RlckZ1bmN0aW9uKGlkZW50aWZpZXI6IHN0cmluZywgaW1wbGVtZW50YXRpb246IEZ1bmN0aW9uKSB7XHJcbiAgaWYgKGludGVybmFsUmVnaXN0ZXJlZEZ1bmN0aW9ucy5oYXNPd25Qcm9wZXJ0eShpZGVudGlmaWVyKSkge1xyXG4gICAgdGhyb3cgbmV3IEVycm9yKGBUaGUgZnVuY3Rpb24gaWRlbnRpZmllciAnJHtpZGVudGlmaWVyfScgaXMgcmVzZXJ2ZWQgYW5kIGNhbm5vdCBiZSByZWdpc3RlcmVkLmApO1xyXG4gIH1cclxuXHJcbiAgaWYgKHJlZ2lzdGVyZWRGdW5jdGlvbnMuaGFzT3duUHJvcGVydHkoaWRlbnRpZmllcikpIHtcclxuICAgIHRocm93IG5ldyBFcnJvcihgQSBmdW5jdGlvbiB3aXRoIHRoZSBpZGVudGlmaWVyICcke2lkZW50aWZpZXJ9JyBoYXMgYWxyZWFkeSBiZWVuIHJlZ2lzdGVyZWQuYCk7XHJcbiAgfVxyXG5cclxuICByZWdpc3RlcmVkRnVuY3Rpb25zW2lkZW50aWZpZXJdID0gaW1wbGVtZW50YXRpb247XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBnZXRSZWdpc3RlcmVkRnVuY3Rpb24oaWRlbnRpZmllcjogc3RyaW5nKTogRnVuY3Rpb24ge1xyXG4gIC8vIEJ5IHByaW9yaXRpc2luZyB0aGUgaW50ZXJuYWwgb25lcywgd2UgZW5zdXJlIHlvdSBjYW4ndCBvdmVycmlkZSB0aGVtXHJcbiAgY29uc3QgcmVzdWx0ID0gaW50ZXJuYWxSZWdpc3RlcmVkRnVuY3Rpb25zW2lkZW50aWZpZXJdIHx8IHJlZ2lzdGVyZWRGdW5jdGlvbnNbaWRlbnRpZmllcl07XHJcbiAgaWYgKHJlc3VsdCkge1xyXG4gICAgcmV0dXJuIHJlc3VsdDtcclxuICB9IGVsc2Uge1xyXG4gICAgdGhyb3cgbmV3IEVycm9yKGBDb3VsZCBub3QgZmluZCByZWdpc3RlcmVkIGZ1bmN0aW9uIHdpdGggbmFtZSAnJHtpZGVudGlmaWVyfScuYCk7XHJcbiAgfVxyXG59XHJcblxuXG5cbi8vIFdFQlBBQ0sgRk9PVEVSIC8vXG4vLyAuL3NyYy9JbnRlcm9wL1JlZ2lzdGVyZWRGdW5jdGlvbi50cyIsImV4cG9ydCBmdW5jdGlvbiBnZXRBc3NlbWJseU5hbWVGcm9tVXJsKHVybDogc3RyaW5nKSB7XHJcbiAgY29uc3QgbGFzdFNlZ21lbnQgPSB1cmwuc3Vic3RyaW5nKHVybC5sYXN0SW5kZXhPZignLycpICsgMSk7XHJcbiAgY29uc3QgcXVlcnlTdHJpbmdTdGFydFBvcyA9IGxhc3RTZWdtZW50LmluZGV4T2YoJz8nKTtcclxuICBjb25zdCBmaWxlbmFtZSA9IHF1ZXJ5U3RyaW5nU3RhcnRQb3MgPCAwID8gbGFzdFNlZ21lbnQgOiBsYXN0U2VnbWVudC5zdWJzdHJpbmcoMCwgcXVlcnlTdHJpbmdTdGFydFBvcyk7XHJcbiAgcmV0dXJuIGZpbGVuYW1lLnJlcGxhY2UoL1xcLmRsbCQvLCAnJyk7XHJcbn1cclxuXG5cblxuLy8gV0VCUEFDSyBGT09URVIgLy9cbi8vIC4vc3JjL1BsYXRmb3JtL0RvdE5ldC50cyIsImltcG9ydCB7IFN5c3RlbV9PYmplY3QsIFN5c3RlbV9TdHJpbmcsIFN5c3RlbV9BcnJheSwgTWV0aG9kSGFuZGxlLCBQb2ludGVyIH0gZnJvbSAnLi4vUGxhdGZvcm0vUGxhdGZvcm0nO1xyXG5pbXBvcnQgeyBwbGF0Zm9ybSB9IGZyb20gJy4uL0Vudmlyb25tZW50JztcclxuaW1wb3J0IHsgcmVuZGVyQmF0Y2ggYXMgcmVuZGVyQmF0Y2hTdHJ1Y3QsIGFycmF5UmFuZ2UsIGFycmF5U2VnbWVudCwgcmVuZGVyVHJlZURpZmZTdHJ1Y3RMZW5ndGgsIHJlbmRlclRyZWVEaWZmLCBSZW5kZXJCYXRjaFBvaW50ZXIsIFJlbmRlclRyZWVEaWZmUG9pbnRlciB9IGZyb20gJy4vUmVuZGVyQmF0Y2gnO1xyXG5pbXBvcnQgeyBCcm93c2VyUmVuZGVyZXIgfSBmcm9tICcuL0Jyb3dzZXJSZW5kZXJlcic7XHJcblxyXG50eXBlIEJyb3dzZXJSZW5kZXJlclJlZ2lzdHJ5ID0geyBbYnJvd3NlclJlbmRlcmVySWQ6IG51bWJlcl06IEJyb3dzZXJSZW5kZXJlciB9O1xyXG5jb25zdCBicm93c2VyUmVuZGVyZXJzOiBCcm93c2VyUmVuZGVyZXJSZWdpc3RyeSA9IHt9O1xyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIGF0dGFjaENvbXBvbmVudFRvRWxlbWVudChicm93c2VyUmVuZGVyZXJJZDogbnVtYmVyLCBlbGVtZW50U2VsZWN0b3I6IFN5c3RlbV9TdHJpbmcsIGNvbXBvbmVudElkOiBudW1iZXIpIHtcclxuICBjb25zdCBlbGVtZW50U2VsZWN0b3JKcyA9IHBsYXRmb3JtLnRvSmF2YVNjcmlwdFN0cmluZyhlbGVtZW50U2VsZWN0b3IpO1xyXG4gIGNvbnN0IGVsZW1lbnQgPSBkb2N1bWVudC5xdWVyeVNlbGVjdG9yKGVsZW1lbnRTZWxlY3RvckpzKTtcclxuICBpZiAoIWVsZW1lbnQpIHtcclxuICAgIHRocm93IG5ldyBFcnJvcihgQ291bGQgbm90IGZpbmQgYW55IGVsZW1lbnQgbWF0Y2hpbmcgc2VsZWN0b3IgJyR7ZWxlbWVudFNlbGVjdG9ySnN9Jy5gKTtcclxuICB9XHJcblxyXG4gIGxldCBicm93c2VyUmVuZGVyZXIgPSBicm93c2VyUmVuZGVyZXJzW2Jyb3dzZXJSZW5kZXJlcklkXTtcclxuICBpZiAoIWJyb3dzZXJSZW5kZXJlcikge1xyXG4gICAgYnJvd3NlclJlbmRlcmVyID0gYnJvd3NlclJlbmRlcmVyc1ticm93c2VyUmVuZGVyZXJJZF0gPSBuZXcgQnJvd3NlclJlbmRlcmVyKGJyb3dzZXJSZW5kZXJlcklkKTtcclxuICB9XHJcbiAgYnJvd3NlclJlbmRlcmVyLmF0dGFjaENvbXBvbmVudFRvRWxlbWVudChjb21wb25lbnRJZCwgZWxlbWVudCk7XHJcbiAgY2xlYXJFbGVtZW50KGVsZW1lbnQpO1xyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gcmVuZGVyQmF0Y2goYnJvd3NlclJlbmRlcmVySWQ6IG51bWJlciwgYmF0Y2g6IFJlbmRlckJhdGNoUG9pbnRlcikge1xyXG4gIGNvbnN0IGJyb3dzZXJSZW5kZXJlciA9IGJyb3dzZXJSZW5kZXJlcnNbYnJvd3NlclJlbmRlcmVySWRdO1xyXG4gIGlmICghYnJvd3NlclJlbmRlcmVyKSB7XHJcbiAgICB0aHJvdyBuZXcgRXJyb3IoYFRoZXJlIGlzIG5vIGJyb3dzZXIgcmVuZGVyZXIgd2l0aCBJRCAke2Jyb3dzZXJSZW5kZXJlcklkfS5gKTtcclxuICB9XHJcbiAgXHJcbiAgY29uc3QgdXBkYXRlZENvbXBvbmVudHMgPSByZW5kZXJCYXRjaFN0cnVjdC51cGRhdGVkQ29tcG9uZW50cyhiYXRjaCk7XHJcbiAgY29uc3QgdXBkYXRlZENvbXBvbmVudHNMZW5ndGggPSBhcnJheVJhbmdlLmNvdW50KHVwZGF0ZWRDb21wb25lbnRzKTtcclxuICBjb25zdCB1cGRhdGVkQ29tcG9uZW50c0FycmF5ID0gYXJyYXlSYW5nZS5hcnJheSh1cGRhdGVkQ29tcG9uZW50cyk7XHJcbiAgY29uc3QgcmVmZXJlbmNlRnJhbWVzU3RydWN0ID0gcmVuZGVyQmF0Y2hTdHJ1Y3QucmVmZXJlbmNlRnJhbWVzKGJhdGNoKTtcclxuICBjb25zdCByZWZlcmVuY2VGcmFtZXMgPSBhcnJheVJhbmdlLmFycmF5KHJlZmVyZW5jZUZyYW1lc1N0cnVjdCk7XHJcblxyXG4gIGZvciAobGV0IGkgPSAwOyBpIDwgdXBkYXRlZENvbXBvbmVudHNMZW5ndGg7IGkrKykge1xyXG4gICAgY29uc3QgZGlmZiA9IHBsYXRmb3JtLmdldEFycmF5RW50cnlQdHIodXBkYXRlZENvbXBvbmVudHNBcnJheSwgaSwgcmVuZGVyVHJlZURpZmZTdHJ1Y3RMZW5ndGgpO1xyXG4gICAgY29uc3QgY29tcG9uZW50SWQgPSByZW5kZXJUcmVlRGlmZi5jb21wb25lbnRJZChkaWZmKTtcclxuXHJcbiAgICBjb25zdCBlZGl0c0FycmF5U2VnbWVudCA9IHJlbmRlclRyZWVEaWZmLmVkaXRzKGRpZmYpO1xyXG4gICAgY29uc3QgZWRpdHMgPSBhcnJheVNlZ21lbnQuYXJyYXkoZWRpdHNBcnJheVNlZ21lbnQpO1xyXG4gICAgY29uc3QgZWRpdHNPZmZzZXQgPSBhcnJheVNlZ21lbnQub2Zmc2V0KGVkaXRzQXJyYXlTZWdtZW50KTtcclxuICAgIGNvbnN0IGVkaXRzTGVuZ3RoID0gYXJyYXlTZWdtZW50LmNvdW50KGVkaXRzQXJyYXlTZWdtZW50KTtcclxuXHJcbiAgICBicm93c2VyUmVuZGVyZXIudXBkYXRlQ29tcG9uZW50KGNvbXBvbmVudElkLCBlZGl0cywgZWRpdHNPZmZzZXQsIGVkaXRzTGVuZ3RoLCByZWZlcmVuY2VGcmFtZXMpO1xyXG4gIH1cclxuXHJcbiAgY29uc3QgZGlzcG9zZWRDb21wb25lbnRJZHMgPSByZW5kZXJCYXRjaFN0cnVjdC5kaXNwb3NlZENvbXBvbmVudElkcyhiYXRjaCk7XHJcbiAgY29uc3QgZGlzcG9zZWRDb21wb25lbnRJZHNMZW5ndGggPSBhcnJheVJhbmdlLmNvdW50KGRpc3Bvc2VkQ29tcG9uZW50SWRzKTtcclxuICBjb25zdCBkaXNwb3NlZENvbXBvbmVudElkc0FycmF5ID0gYXJyYXlSYW5nZS5hcnJheShkaXNwb3NlZENvbXBvbmVudElkcyk7XHJcbiAgZm9yIChsZXQgaSA9IDA7IGkgPCBkaXNwb3NlZENvbXBvbmVudElkc0xlbmd0aDsgaSsrKSB7XHJcbiAgICBjb25zdCBjb21wb25lbnRJZFB0ciA9IHBsYXRmb3JtLmdldEFycmF5RW50cnlQdHIoZGlzcG9zZWRDb21wb25lbnRJZHNBcnJheSwgaSwgNCk7XHJcbiAgICBjb25zdCBjb21wb25lbnRJZCA9IHBsYXRmb3JtLnJlYWRJbnQzMkZpZWxkKGNvbXBvbmVudElkUHRyKTtcclxuICAgIGJyb3dzZXJSZW5kZXJlci5kaXNwb3NlQ29tcG9uZW50KGNvbXBvbmVudElkKTtcclxuICB9XHJcbn1cclxuXHJcbmZ1bmN0aW9uIGNsZWFyRWxlbWVudChlbGVtZW50OiBFbGVtZW50KSB7XHJcbiAgbGV0IGNoaWxkTm9kZTogTm9kZSB8IG51bGw7XHJcbiAgd2hpbGUgKGNoaWxkTm9kZSA9IGVsZW1lbnQuZmlyc3RDaGlsZCkge1xyXG4gICAgZWxlbWVudC5yZW1vdmVDaGlsZChjaGlsZE5vZGUpO1xyXG4gIH1cclxufVxyXG5cblxuXG4vLyBXRUJQQUNLIEZPT1RFUiAvL1xuLy8gLi9zcmMvUmVuZGVyaW5nL1JlbmRlcmVyLnRzIiwiaW1wb3J0IHsgcGxhdGZvcm0gfSBmcm9tICcuL0Vudmlyb25tZW50JztcclxuaW1wb3J0IHsgZ2V0QXNzZW1ibHlOYW1lRnJvbVVybCB9IGZyb20gJy4vUGxhdGZvcm0vRG90TmV0JztcclxuaW1wb3J0ICcuL1JlbmRlcmluZy9SZW5kZXJlcic7XHJcbmltcG9ydCAnLi9TZXJ2aWNlcy9IdHRwJztcclxuaW1wb3J0ICcuL1NlcnZpY2VzL1VyaUhlbHBlcic7XHJcbmltcG9ydCAnLi9HbG9iYWxFeHBvcnRzJztcclxuXHJcbmFzeW5jIGZ1bmN0aW9uIGJvb3QoKSB7XHJcbiAgLy8gUmVhZCBzdGFydHVwIGNvbmZpZyBmcm9tIHRoZSA8c2NyaXB0PiBlbGVtZW50IHRoYXQncyBpbXBvcnRpbmcgdGhpcyBmaWxlXHJcbiAgY29uc3QgYWxsU2NyaXB0RWxlbXMgPSBkb2N1bWVudC5nZXRFbGVtZW50c0J5VGFnTmFtZSgnc2NyaXB0Jyk7XHJcbiAgY29uc3QgdGhpc1NjcmlwdEVsZW0gPSAoZG9jdW1lbnQuY3VycmVudFNjcmlwdCB8fCBhbGxTY3JpcHRFbGVtc1thbGxTY3JpcHRFbGVtcy5sZW5ndGggLSAxXSkgYXMgSFRNTFNjcmlwdEVsZW1lbnQ7XHJcbiAgY29uc3QgZW50cnlQb2ludERsbCA9IGdldFJlcXVpcmVkQm9vdFNjcmlwdEF0dHJpYnV0ZSh0aGlzU2NyaXB0RWxlbSwgJ21haW4nKTtcclxuICBjb25zdCBlbnRyeVBvaW50TWV0aG9kID0gZ2V0UmVxdWlyZWRCb290U2NyaXB0QXR0cmlidXRlKHRoaXNTY3JpcHRFbGVtLCAnZW50cnlwb2ludCcpO1xyXG4gIGNvbnN0IGVudHJ5UG9pbnRBc3NlbWJseU5hbWUgPSBnZXRBc3NlbWJseU5hbWVGcm9tVXJsKGVudHJ5UG9pbnREbGwpO1xyXG4gIGNvbnN0IHJlZmVyZW5jZUFzc2VtYmxpZXNDb21tYVNlcGFyYXRlZCA9IHRoaXNTY3JpcHRFbGVtLmdldEF0dHJpYnV0ZSgncmVmZXJlbmNlcycpIHx8ICcnO1xyXG4gIGNvbnN0IHJlZmVyZW5jZUFzc2VtYmxpZXMgPSByZWZlcmVuY2VBc3NlbWJsaWVzQ29tbWFTZXBhcmF0ZWRcclxuICAgIC5zcGxpdCgnLCcpXHJcbiAgICAubWFwKHMgPT4gcy50cmltKCkpXHJcbiAgICAuZmlsdGVyKHMgPT4gISFzKTtcclxuXHJcbiAgLy8gRGV0ZXJtaW5lIHRoZSBVUkxzIG9mIHRoZSBhc3NlbWJsaWVzIHdlIHdhbnQgdG8gbG9hZFxyXG4gIGNvbnN0IGxvYWRBc3NlbWJseVVybHMgPSBbZW50cnlQb2ludERsbF1cclxuICAgIC5jb25jYXQocmVmZXJlbmNlQXNzZW1ibGllcylcclxuICAgIC5tYXAoZmlsZW5hbWUgPT4gYF9mcmFtZXdvcmsvX2Jpbi8ke2ZpbGVuYW1lfWApO1xyXG5cclxuICB0cnkge1xyXG4gICAgYXdhaXQgcGxhdGZvcm0uc3RhcnQobG9hZEFzc2VtYmx5VXJscyk7XHJcbiAgfSBjYXRjaCAoZXgpIHtcclxuICAgIHRocm93IG5ldyBFcnJvcihgRmFpbGVkIHRvIHN0YXJ0IHBsYXRmb3JtLiBSZWFzb246ICR7ZXh9YCk7XHJcbiAgfVxyXG5cclxuICAvLyBTdGFydCB1cCB0aGUgYXBwbGljYXRpb25cclxuICBwbGF0Zm9ybS5jYWxsRW50cnlQb2ludChlbnRyeVBvaW50QXNzZW1ibHlOYW1lLCBlbnRyeVBvaW50TWV0aG9kLCBbXSk7XHJcbn1cclxuXHJcbmZ1bmN0aW9uIGdldFJlcXVpcmVkQm9vdFNjcmlwdEF0dHJpYnV0ZShlbGVtOiBIVE1MU2NyaXB0RWxlbWVudCwgYXR0cmlidXRlTmFtZTogc3RyaW5nKTogc3RyaW5nIHtcclxuICBjb25zdCByZXN1bHQgPSBlbGVtLmdldEF0dHJpYnV0ZShhdHRyaWJ1dGVOYW1lKTtcclxuICBpZiAoIXJlc3VsdCkge1xyXG4gICAgdGhyb3cgbmV3IEVycm9yKGBNaXNzaW5nIFwiJHthdHRyaWJ1dGVOYW1lfVwiIGF0dHJpYnV0ZSBvbiBCbGF6b3Igc2NyaXB0IHRhZy5gKTtcclxuICB9XHJcbiAgcmV0dXJuIHJlc3VsdDtcclxufVxyXG5cclxuYm9vdCgpO1xyXG5cblxuXG4vLyBXRUJQQUNLIEZPT1RFUiAvL1xuLy8gLi9zcmMvQm9vdC50cyIsImltcG9ydCB7IE1ldGhvZEhhbmRsZSwgU3lzdGVtX09iamVjdCwgU3lzdGVtX1N0cmluZywgU3lzdGVtX0FycmF5LCBQb2ludGVyLCBQbGF0Zm9ybSB9IGZyb20gJy4uL1BsYXRmb3JtJztcclxuaW1wb3J0IHsgZ2V0QXNzZW1ibHlOYW1lRnJvbVVybCB9IGZyb20gJy4uL0RvdE5ldCc7XHJcbmltcG9ydCB7IGdldFJlZ2lzdGVyZWRGdW5jdGlvbiB9IGZyb20gJy4uLy4uL0ludGVyb3AvUmVnaXN0ZXJlZEZ1bmN0aW9uJztcclxuXHJcbmxldCBhc3NlbWJseV9sb2FkOiAoYXNzZW1ibHlOYW1lOiBzdHJpbmcpID0+IG51bWJlcjtcclxubGV0IGZpbmRfY2xhc3M6IChhc3NlbWJseUhhbmRsZTogbnVtYmVyLCBuYW1lc3BhY2U6IHN0cmluZywgY2xhc3NOYW1lOiBzdHJpbmcpID0+IG51bWJlcjtcclxubGV0IGZpbmRfbWV0aG9kOiAodHlwZUhhbmRsZTogbnVtYmVyLCBtZXRob2ROYW1lOiBzdHJpbmcsIHVua25vd25Bcmc6IG51bWJlcikgPT4gTWV0aG9kSGFuZGxlO1xyXG5sZXQgaW52b2tlX21ldGhvZDogKG1ldGhvZDogTWV0aG9kSGFuZGxlLCB0YXJnZXQ6IFN5c3RlbV9PYmplY3QsIGFyZ3NBcnJheVB0cjogbnVtYmVyLCBleGNlcHRpb25GbGFnSW50UHRyOiBudW1iZXIpID0+IFN5c3RlbV9PYmplY3Q7XHJcbmxldCBtb25vX3N0cmluZ19nZXRfdXRmODogKG1hbmFnZWRTdHJpbmc6IFN5c3RlbV9TdHJpbmcpID0+IE1vbm8uVXRmOFB0cjtcclxubGV0IG1vbm9fc3RyaW5nOiAoanNTdHJpbmc6IHN0cmluZykgPT4gU3lzdGVtX1N0cmluZztcclxuXHJcbmV4cG9ydCBjb25zdCBtb25vUGxhdGZvcm06IFBsYXRmb3JtID0ge1xyXG4gIHN0YXJ0OiBmdW5jdGlvbiBzdGFydChsb2FkQXNzZW1ibHlVcmxzOiBzdHJpbmdbXSkge1xyXG4gICAgcmV0dXJuIG5ldyBQcm9taXNlPHZvaWQ+KChyZXNvbHZlLCByZWplY3QpID0+IHtcclxuICAgICAgLy8gbW9uby5qcyBhc3N1bWVzIHRoZSBleGlzdGVuY2Ugb2YgdGhpc1xyXG4gICAgICB3aW5kb3dbJ0Jyb3dzZXInXSA9IHtcclxuICAgICAgICBpbml0OiAoKSA9PiB7IH0sXHJcbiAgICAgICAgYXN5bmNMb2FkOiBhc3luY0xvYWRcclxuICAgICAgfTtcclxuXHJcbiAgICAgIC8vIEVtc2NyaXB0ZW4gd29ya3MgYnkgZXhwZWN0aW5nIHRoZSBtb2R1bGUgY29uZmlnIHRvIGJlIGEgZ2xvYmFsXHJcbiAgICAgIHdpbmRvd1snTW9kdWxlJ10gPSBjcmVhdGVFbXNjcmlwdGVuTW9kdWxlSW5zdGFuY2UobG9hZEFzc2VtYmx5VXJscywgcmVzb2x2ZSwgcmVqZWN0KTtcclxuXHJcbiAgICAgIGFkZFNjcmlwdFRhZ3NUb0RvY3VtZW50KCk7XHJcbiAgICB9KTtcclxuICB9LFxyXG5cclxuICBmaW5kTWV0aG9kOiBmdW5jdGlvbiBmaW5kTWV0aG9kKGFzc2VtYmx5TmFtZTogc3RyaW5nLCBuYW1lc3BhY2U6IHN0cmluZywgY2xhc3NOYW1lOiBzdHJpbmcsIG1ldGhvZE5hbWU6IHN0cmluZyk6IE1ldGhvZEhhbmRsZSB7XHJcbiAgICAvLyBUT0RPOiBDYWNoZSB0aGUgYXNzZW1ibHlfbG9hZCBvdXRwdXRzP1xyXG4gICAgY29uc3QgYXNzZW1ibHlIYW5kbGUgPSBhc3NlbWJseV9sb2FkKGFzc2VtYmx5TmFtZSk7XHJcbiAgICBpZiAoIWFzc2VtYmx5SGFuZGxlKSB7XHJcbiAgICAgIHRocm93IG5ldyBFcnJvcihgQ291bGQgbm90IGZpbmQgYXNzZW1ibHkgXCIke2Fzc2VtYmx5TmFtZX1cImApO1xyXG4gICAgfVxyXG5cclxuICAgIGNvbnN0IHR5cGVIYW5kbGUgPSBmaW5kX2NsYXNzKGFzc2VtYmx5SGFuZGxlLCBuYW1lc3BhY2UsIGNsYXNzTmFtZSk7XHJcbiAgICBpZiAoIXR5cGVIYW5kbGUpIHtcclxuICAgICAgdGhyb3cgbmV3IEVycm9yKGBDb3VsZCBub3QgZmluZCB0eXBlIFwiJHtjbGFzc05hbWV9XCIgaW4gbmFtZXNwYWNlIFwiJHtuYW1lc3BhY2V9XCIgaW4gYXNzZW1ibHkgXCIke2Fzc2VtYmx5TmFtZX1cImApO1xyXG4gICAgfVxyXG5cclxuICAgIGNvbnN0IG1ldGhvZEhhbmRsZSA9IGZpbmRfbWV0aG9kKHR5cGVIYW5kbGUsIG1ldGhvZE5hbWUsIC0xKTtcclxuICAgIGlmICghbWV0aG9kSGFuZGxlKSB7XHJcbiAgICAgIHRocm93IG5ldyBFcnJvcihgQ291bGQgbm90IGZpbmQgbWV0aG9kIFwiJHttZXRob2ROYW1lfVwiIG9uIHR5cGUgXCIke25hbWVzcGFjZX0uJHtjbGFzc05hbWV9XCJgKTtcclxuICAgIH1cclxuXHJcbiAgICByZXR1cm4gbWV0aG9kSGFuZGxlO1xyXG4gIH0sXHJcblxyXG4gIGNhbGxFbnRyeVBvaW50OiBmdW5jdGlvbiBjYWxsRW50cnlQb2ludChhc3NlbWJseU5hbWU6IHN0cmluZywgZW50cnlwb2ludE1ldGhvZDogc3RyaW5nLCBhcmdzOiBTeXN0ZW1fT2JqZWN0W10pOiB2b2lkIHtcclxuICAgIC8vIFBhcnNlIHRoZSBlbnRyeXBvaW50TWV0aG9kLCB3aGljaCBpcyBvZiB0aGUgZm9ybSBNeUFwcC5NeU5hbWVzcGFjZS5NeVR5cGVOYW1lOjpNeU1ldGhvZE5hbWVcclxuICAgIC8vIE5vdGUgdGhhdCB3ZSBkb24ndCBzdXBwb3J0IHNwZWNpZnlpbmcgYSBtZXRob2Qgb3ZlcmxvYWQsIHNvIGl0IGhhcyB0byBiZSB1bmlxdWVcclxuICAgIGNvbnN0IGVudHJ5cG9pbnRTZWdtZW50cyA9IGVudHJ5cG9pbnRNZXRob2Quc3BsaXQoJzo6Jyk7XHJcbiAgICBpZiAoZW50cnlwb2ludFNlZ21lbnRzLmxlbmd0aCAhPSAyKSB7XHJcbiAgICAgIHRocm93IG5ldyBFcnJvcignTWFsZm9ybWVkIGVudHJ5IHBvaW50IG1ldGhvZCBuYW1lOyBjb3VsZCBub3QgcmVzb2x2ZSBjbGFzcyBuYW1lIGFuZCBtZXRob2QgbmFtZS4nKTtcclxuICAgIH1cclxuICAgIGNvbnN0IHR5cGVGdWxsTmFtZSA9IGVudHJ5cG9pbnRTZWdtZW50c1swXTtcclxuICAgIGNvbnN0IG1ldGhvZE5hbWUgPSBlbnRyeXBvaW50U2VnbWVudHNbMV07XHJcbiAgICBjb25zdCBsYXN0RG90ID0gdHlwZUZ1bGxOYW1lLmxhc3RJbmRleE9mKCcuJyk7XHJcbiAgICBjb25zdCBuYW1lc3BhY2UgPSBsYXN0RG90ID4gLTEgPyB0eXBlRnVsbE5hbWUuc3Vic3RyaW5nKDAsIGxhc3REb3QpIDogJyc7XHJcbiAgICBjb25zdCB0eXBlU2hvcnROYW1lID0gbGFzdERvdCA+IC0xID8gdHlwZUZ1bGxOYW1lLnN1YnN0cmluZyhsYXN0RG90ICsgMSkgOiB0eXBlRnVsbE5hbWU7XHJcblxyXG4gICAgY29uc3QgZW50cnlQb2ludE1ldGhvZEhhbmRsZSA9IG1vbm9QbGF0Zm9ybS5maW5kTWV0aG9kKGFzc2VtYmx5TmFtZSwgbmFtZXNwYWNlLCB0eXBlU2hvcnROYW1lLCBtZXRob2ROYW1lKTtcclxuICAgIG1vbm9QbGF0Zm9ybS5jYWxsTWV0aG9kKGVudHJ5UG9pbnRNZXRob2RIYW5kbGUsIG51bGwsIGFyZ3MpO1xyXG4gIH0sXHJcblxyXG4gIGNhbGxNZXRob2Q6IGZ1bmN0aW9uIGNhbGxNZXRob2QobWV0aG9kOiBNZXRob2RIYW5kbGUsIHRhcmdldDogU3lzdGVtX09iamVjdCwgYXJnczogU3lzdGVtX09iamVjdFtdKTogU3lzdGVtX09iamVjdCB7XHJcbiAgICBpZiAoYXJncy5sZW5ndGggPiA0KSB7XHJcbiAgICAgIC8vIEhvcGVmdWxseSB0aGlzIHJlc3RyaWN0aW9uIGNhbiBiZSBlYXNlZCBzb29uLCBidXQgZm9yIG5vdyBtYWtlIGl0IGNsZWFyIHdoYXQncyBnb2luZyBvblxyXG4gICAgICB0aHJvdyBuZXcgRXJyb3IoYEN1cnJlbnRseSwgTW9ub1BsYXRmb3JtIHN1cHBvcnRzIHBhc3NpbmcgYSBtYXhpbXVtIG9mIDQgYXJndW1lbnRzIGZyb20gSlMgdG8gLk5FVC4gWW91IHRyaWVkIHRvIHBhc3MgJHthcmdzLmxlbmd0aH0uYCk7XHJcbiAgICB9XHJcblxyXG4gICAgY29uc3Qgc3RhY2sgPSBNb2R1bGUuc3RhY2tTYXZlKCk7XHJcblxyXG4gICAgdHJ5IHtcclxuICAgICAgY29uc3QgYXJnc0J1ZmZlciA9IE1vZHVsZS5zdGFja0FsbG9jKGFyZ3MubGVuZ3RoKTtcclxuICAgICAgY29uc3QgZXhjZXB0aW9uRmxhZ01hbmFnZWRJbnQgPSBNb2R1bGUuc3RhY2tBbGxvYyg0KTtcclxuICAgICAgZm9yICh2YXIgaSA9IDA7IGkgPCBhcmdzLmxlbmd0aDsgKytpKSB7XHJcbiAgICAgICAgTW9kdWxlLnNldFZhbHVlKGFyZ3NCdWZmZXIgKyBpICogNCwgYXJnc1tpXSwgJ2kzMicpO1xyXG4gICAgICB9XHJcbiAgICAgIE1vZHVsZS5zZXRWYWx1ZShleGNlcHRpb25GbGFnTWFuYWdlZEludCwgMCwgJ2kzMicpO1xyXG5cclxuICAgICAgY29uc3QgcmVzID0gaW52b2tlX21ldGhvZChtZXRob2QsIHRhcmdldCwgYXJnc0J1ZmZlciwgZXhjZXB0aW9uRmxhZ01hbmFnZWRJbnQpO1xyXG5cclxuICAgICAgaWYgKE1vZHVsZS5nZXRWYWx1ZShleGNlcHRpb25GbGFnTWFuYWdlZEludCwgJ2kzMicpICE9PSAwKSB7XHJcbiAgICAgICAgLy8gSWYgdGhlIGV4Y2VwdGlvbiBmbGFnIGlzIHNldCwgdGhlIHJldHVybmVkIHZhbHVlIGlzIGV4Y2VwdGlvbi5Ub1N0cmluZygpXHJcbiAgICAgICAgdGhyb3cgbmV3IEVycm9yKG1vbm9QbGF0Zm9ybS50b0phdmFTY3JpcHRTdHJpbmcoPFN5c3RlbV9TdHJpbmc+cmVzKSk7XHJcbiAgICAgIH1cclxuXHJcbiAgICAgIHJldHVybiByZXM7XHJcbiAgICB9IGZpbmFsbHkge1xyXG4gICAgICBNb2R1bGUuc3RhY2tSZXN0b3JlKHN0YWNrKTtcclxuICAgIH1cclxuICB9LFxyXG5cclxuICB0b0phdmFTY3JpcHRTdHJpbmc6IGZ1bmN0aW9uIHRvSmF2YVNjcmlwdFN0cmluZyhtYW5hZ2VkU3RyaW5nOiBTeXN0ZW1fU3RyaW5nKSB7XHJcbiAgICAvLyBDb21tZW50cyBmcm9tIG9yaWdpbmFsIE1vbm8gc2FtcGxlOlxyXG4gICAgLy9GSVhNRSB0aGlzIGlzIHdhc3RlZnVsbCwgd2UgY291bGQgcmVtb3ZlIHRoZSB0ZW1wIG1hbGxvYyBieSBnb2luZyB0aGUgVVRGMTYgcm91dGVcclxuICAgIC8vRklYTUUgdGhpcyBpcyB1bnNhZmUsIGN1eiByYXcgb2JqZWN0cyBjb3VsZCBiZSBHQydkLlxyXG5cclxuICAgIGNvbnN0IHV0ZjggPSBtb25vX3N0cmluZ19nZXRfdXRmOChtYW5hZ2VkU3RyaW5nKTtcclxuICAgIGNvbnN0IHJlcyA9IE1vZHVsZS5VVEY4VG9TdHJpbmcodXRmOCk7XHJcbiAgICBNb2R1bGUuX2ZyZWUodXRmOCBhcyBhbnkpO1xyXG4gICAgcmV0dXJuIHJlcztcclxuICB9LFxyXG5cclxuICB0b0RvdE5ldFN0cmluZzogZnVuY3Rpb24gdG9Eb3ROZXRTdHJpbmcoanNTdHJpbmc6IHN0cmluZyk6IFN5c3RlbV9TdHJpbmcge1xyXG4gICAgcmV0dXJuIG1vbm9fc3RyaW5nKGpzU3RyaW5nKTtcclxuICB9LFxyXG5cclxuICBnZXRBcnJheUxlbmd0aDogZnVuY3Rpb24gZ2V0QXJyYXlMZW5ndGgoYXJyYXk6IFN5c3RlbV9BcnJheTxhbnk+KTogbnVtYmVyIHtcclxuICAgIHJldHVybiBNb2R1bGUuZ2V0VmFsdWUoZ2V0QXJyYXlEYXRhUG9pbnRlcihhcnJheSksICdpMzInKTtcclxuICB9LFxyXG5cclxuICBnZXRBcnJheUVudHJ5UHRyOiBmdW5jdGlvbiBnZXRBcnJheUVudHJ5UHRyPFRQdHIgZXh0ZW5kcyBQb2ludGVyPihhcnJheTogU3lzdGVtX0FycmF5PFRQdHI+LCBpbmRleDogbnVtYmVyLCBpdGVtU2l6ZTogbnVtYmVyKTogVFB0ciB7XHJcbiAgICAvLyBGaXJzdCBieXRlIGlzIGFycmF5IGxlbmd0aCwgZm9sbG93ZWQgYnkgZW50cmllc1xyXG4gICAgY29uc3QgYWRkcmVzcyA9IGdldEFycmF5RGF0YVBvaW50ZXIoYXJyYXkpICsgNCArIGluZGV4ICogaXRlbVNpemU7XHJcbiAgICByZXR1cm4gYWRkcmVzcyBhcyBhbnkgYXMgVFB0cjtcclxuICB9LFxyXG5cclxuICBnZXRPYmplY3RGaWVsZHNCYXNlQWRkcmVzczogZnVuY3Rpb24gZ2V0T2JqZWN0RmllbGRzQmFzZUFkZHJlc3MocmVmZXJlbmNlVHlwZWRPYmplY3Q6IFN5c3RlbV9PYmplY3QpOiBQb2ludGVyIHtcclxuICAgIC8vIFRoZSBmaXJzdCB0d28gaW50MzIgdmFsdWVzIGFyZSBpbnRlcm5hbCBNb25vIGRhdGFcclxuICAgIHJldHVybiAocmVmZXJlbmNlVHlwZWRPYmplY3QgYXMgYW55IGFzIG51bWJlciArIDgpIGFzIGFueSBhcyBQb2ludGVyO1xyXG4gIH0sXHJcblxyXG4gIHJlYWRJbnQzMkZpZWxkOiBmdW5jdGlvbiByZWFkSGVhcEludDMyKGJhc2VBZGRyZXNzOiBQb2ludGVyLCBmaWVsZE9mZnNldD86IG51bWJlcik6IG51bWJlciB7XHJcbiAgICByZXR1cm4gTW9kdWxlLmdldFZhbHVlKChiYXNlQWRkcmVzcyBhcyBhbnkgYXMgbnVtYmVyKSArIChmaWVsZE9mZnNldCB8fCAwKSwgJ2kzMicpO1xyXG4gIH0sXHJcblxyXG4gIHJlYWRPYmplY3RGaWVsZDogZnVuY3Rpb24gcmVhZEhlYXBPYmplY3Q8VCBleHRlbmRzIFN5c3RlbV9PYmplY3Q+KGJhc2VBZGRyZXNzOiBQb2ludGVyLCBmaWVsZE9mZnNldD86IG51bWJlcik6IFQge1xyXG4gICAgcmV0dXJuIE1vZHVsZS5nZXRWYWx1ZSgoYmFzZUFkZHJlc3MgYXMgYW55IGFzIG51bWJlcikgKyAoZmllbGRPZmZzZXQgfHwgMCksICdpMzInKSBhcyBhbnkgYXMgVDtcclxuICB9LFxyXG5cclxuICByZWFkU3RyaW5nRmllbGQ6IGZ1bmN0aW9uIHJlYWRIZWFwT2JqZWN0KGJhc2VBZGRyZXNzOiBQb2ludGVyLCBmaWVsZE9mZnNldD86IG51bWJlcik6IHN0cmluZyB8IG51bGwge1xyXG4gICAgY29uc3QgZmllbGRWYWx1ZSA9IE1vZHVsZS5nZXRWYWx1ZSgoYmFzZUFkZHJlc3MgYXMgYW55IGFzIG51bWJlcikgKyAoZmllbGRPZmZzZXQgfHwgMCksICdpMzInKTtcclxuICAgIHJldHVybiBmaWVsZFZhbHVlID09PSAwID8gbnVsbCA6IG1vbm9QbGF0Zm9ybS50b0phdmFTY3JpcHRTdHJpbmcoZmllbGRWYWx1ZSBhcyBhbnkgYXMgU3lzdGVtX1N0cmluZyk7XHJcbiAgfSxcclxuXHJcbiAgcmVhZFN0cnVjdEZpZWxkOiBmdW5jdGlvbiByZWFkU3RydWN0RmllbGQ8VCBleHRlbmRzIFBvaW50ZXI+KGJhc2VBZGRyZXNzOiBQb2ludGVyLCBmaWVsZE9mZnNldD86IG51bWJlcik6IFQge1xyXG4gICAgcmV0dXJuICgoYmFzZUFkZHJlc3MgYXMgYW55IGFzIG51bWJlcikgKyAoZmllbGRPZmZzZXQgfHwgMCkpIGFzIGFueSBhcyBUO1xyXG4gIH0sXHJcbn07XHJcblxyXG4vLyBCeXBhc3Mgbm9ybWFsIHR5cGUgY2hlY2tpbmcgdG8gYWRkIHRoaXMgZXh0cmEgZnVuY3Rpb24uIEl0J3Mgb25seSBpbnRlbmRlZCB0byBiZSBjYWxsZWQgZnJvbVxyXG4vLyB0aGUgSlMgY29kZSBpbiBNb25vJ3MgZHJpdmVyLmMuIEl0J3MgbmV2ZXIgaW50ZW5kZWQgdG8gYmUgY2FsbGVkIGZyb20gVHlwZVNjcmlwdC5cclxuKG1vbm9QbGF0Zm9ybSBhcyBhbnkpLm1vbm9HZXRSZWdpc3RlcmVkRnVuY3Rpb24gPSBnZXRSZWdpc3RlcmVkRnVuY3Rpb247XHJcblxyXG5mdW5jdGlvbiBhZGRTY3JpcHRUYWdzVG9Eb2N1bWVudCgpIHtcclxuICAvLyBMb2FkIGVpdGhlciB0aGUgd2FzbSBvciBhc20uanMgdmVyc2lvbiBvZiB0aGUgTW9ubyBydW50aW1lXHJcbiAgY29uc3QgYnJvd3NlclN1cHBvcnRzTmF0aXZlV2ViQXNzZW1ibHkgPSB0eXBlb2YgV2ViQXNzZW1ibHkgIT09ICd1bmRlZmluZWQnICYmIFdlYkFzc2VtYmx5LnZhbGlkYXRlO1xyXG4gIGNvbnN0IG1vbm9SdW50aW1lVXJsQmFzZSA9ICdfZnJhbWV3b3JrLycgKyAoYnJvd3NlclN1cHBvcnRzTmF0aXZlV2ViQXNzZW1ibHkgPyAnd2FzbScgOiAnYXNtanMnKTtcclxuICBjb25zdCBtb25vUnVudGltZVNjcmlwdFVybCA9IGAke21vbm9SdW50aW1lVXJsQmFzZX0vbW9uby5qc2A7XHJcblxyXG4gIGlmICghYnJvd3NlclN1cHBvcnRzTmF0aXZlV2ViQXNzZW1ibHkpIHtcclxuICAgIC8vIEluIHRoZSBhc21qcyBjYXNlLCB0aGUgaW5pdGlhbCBtZW1vcnkgc3RydWN0dXJlIGlzIGluIGEgc2VwYXJhdGUgZmlsZSB3ZSBuZWVkIHRvIGRvd25sb2FkXHJcbiAgICBjb25zdCBtZW1pbml0WEhSID0gTW9kdWxlWydtZW1vcnlJbml0aWFsaXplclJlcXVlc3QnXSA9IG5ldyBYTUxIdHRwUmVxdWVzdCgpO1xyXG4gICAgbWVtaW5pdFhIUi5vcGVuKCdHRVQnLCBgJHttb25vUnVudGltZVVybEJhc2V9L21vbm8uanMubWVtYCk7XHJcbiAgICBtZW1pbml0WEhSLnJlc3BvbnNlVHlwZSA9ICdhcnJheWJ1ZmZlcic7XHJcbiAgICBtZW1pbml0WEhSLnNlbmQobnVsbCk7XHJcbiAgfVxyXG5cclxuICBkb2N1bWVudC53cml0ZShgPHNjcmlwdCBkZWZlciBzcmM9XCIke21vbm9SdW50aW1lU2NyaXB0VXJsfVwiPjwvc2NyaXB0PmApO1xyXG59XHJcblxyXG5mdW5jdGlvbiBjcmVhdGVFbXNjcmlwdGVuTW9kdWxlSW5zdGFuY2UobG9hZEFzc2VtYmx5VXJsczogc3RyaW5nW10sIG9uUmVhZHk6ICgpID0+IHZvaWQsIG9uRXJyb3I6IChyZWFzb24/OiBhbnkpID0+IHZvaWQpIHtcclxuICBjb25zdCBtb2R1bGUgPSB7fSBhcyB0eXBlb2YgTW9kdWxlO1xyXG4gIGNvbnN0IHdhc21CaW5hcnlGaWxlID0gJ19mcmFtZXdvcmsvd2FzbS9tb25vLndhc20nO1xyXG4gIGNvbnN0IGFzbWpzQ29kZUZpbGUgPSAnX2ZyYW1ld29yay9hc21qcy9tb25vLmFzbS5qcyc7XHJcblxyXG4gIG1vZHVsZS5wcmludCA9IGxpbmUgPT4gY29uc29sZS5sb2coYFdBU006ICR7bGluZX1gKTtcclxuICBtb2R1bGUucHJpbnRFcnIgPSBsaW5lID0+IGNvbnNvbGUuZXJyb3IoYFdBU006ICR7bGluZX1gKTtcclxuICBtb2R1bGUucHJlUnVuID0gW107XHJcbiAgbW9kdWxlLnBvc3RSdW4gPSBbXTtcclxuICBtb2R1bGUucHJlbG9hZFBsdWdpbnMgPSBbXTtcclxuXHJcbiAgbW9kdWxlLmxvY2F0ZUZpbGUgPSBmaWxlTmFtZSA9PiB7XHJcbiAgICBzd2l0Y2ggKGZpbGVOYW1lKSB7XHJcbiAgICAgIGNhc2UgJ21vbm8ud2FzbSc6IHJldHVybiB3YXNtQmluYXJ5RmlsZTtcclxuICAgICAgY2FzZSAnbW9uby5hc20uanMnOiByZXR1cm4gYXNtanNDb2RlRmlsZTtcclxuICAgICAgZGVmYXVsdDogcmV0dXJuIGZpbGVOYW1lO1xyXG4gICAgfVxyXG4gIH07XHJcblxyXG4gIG1vZHVsZS5wcmVSdW4ucHVzaCgoKSA9PiB7XHJcbiAgICAvLyBCeSBub3csIGVtc2NyaXB0ZW4gc2hvdWxkIGJlIGluaXRpYWxpc2VkIGVub3VnaCB0aGF0IHdlIGNhbiBjYXB0dXJlIHRoZXNlIG1ldGhvZHMgZm9yIGxhdGVyIHVzZVxyXG4gICAgYXNzZW1ibHlfbG9hZCA9IE1vZHVsZS5jd3JhcCgnbW9ub193YXNtX2Fzc2VtYmx5X2xvYWQnLCAnbnVtYmVyJywgWydzdHJpbmcnXSk7XHJcbiAgICBmaW5kX2NsYXNzID0gTW9kdWxlLmN3cmFwKCdtb25vX3dhc21fYXNzZW1ibHlfZmluZF9jbGFzcycsICdudW1iZXInLCBbJ251bWJlcicsICdzdHJpbmcnLCAnc3RyaW5nJ10pO1xyXG4gICAgZmluZF9tZXRob2QgPSBNb2R1bGUuY3dyYXAoJ21vbm9fd2FzbV9hc3NlbWJseV9maW5kX21ldGhvZCcsICdudW1iZXInLCBbJ251bWJlcicsICdzdHJpbmcnLCAnbnVtYmVyJ10pO1xyXG4gICAgaW52b2tlX21ldGhvZCA9IE1vZHVsZS5jd3JhcCgnbW9ub193YXNtX2ludm9rZV9tZXRob2QnLCAnbnVtYmVyJywgWydudW1iZXInLCAnbnVtYmVyJywgJ251bWJlciddKTtcclxuICAgIG1vbm9fc3RyaW5nX2dldF91dGY4ID0gTW9kdWxlLmN3cmFwKCdtb25vX3dhc21fc3RyaW5nX2dldF91dGY4JywgJ251bWJlcicsIFsnbnVtYmVyJ10pO1xyXG4gICAgbW9ub19zdHJpbmcgPSBNb2R1bGUuY3dyYXAoJ21vbm9fd2FzbV9zdHJpbmdfZnJvbV9qcycsICdudW1iZXInLCBbJ3N0cmluZyddKTtcclxuXHJcbiAgICBNb2R1bGUuRlNfY3JlYXRlUGF0aCgnLycsICdhcHBCaW5EaXInLCB0cnVlLCB0cnVlKTtcclxuICAgIGxvYWRBc3NlbWJseVVybHMuZm9yRWFjaCh1cmwgPT5cclxuICAgICAgRlMuY3JlYXRlUHJlbG9hZGVkRmlsZSgnYXBwQmluRGlyJywgYCR7Z2V0QXNzZW1ibHlOYW1lRnJvbVVybCh1cmwpfS5kbGxgLCB1cmwsIHRydWUsIGZhbHNlLCB1bmRlZmluZWQsIG9uRXJyb3IpKTtcclxuICB9KTtcclxuXHJcbiAgbW9kdWxlLnBvc3RSdW4ucHVzaCgoKSA9PiB7XHJcbiAgICBjb25zdCBsb2FkX3J1bnRpbWUgPSBNb2R1bGUuY3dyYXAoJ21vbm9fd2FzbV9sb2FkX3J1bnRpbWUnLCBudWxsLCBbJ3N0cmluZyddKTtcclxuICAgIGxvYWRfcnVudGltZSgnYXBwQmluRGlyJyk7XHJcbiAgICBvblJlYWR5KCk7XHJcbiAgfSk7XHJcblxyXG4gIHJldHVybiBtb2R1bGU7XHJcbn1cclxuXHJcbmZ1bmN0aW9uIGFzeW5jTG9hZCh1cmwsIG9ubG9hZCwgb25lcnJvcikge1xyXG4gIHZhciB4aHIgPSBuZXcgWE1MSHR0cFJlcXVlc3Q7XHJcbiAgeGhyLm9wZW4oJ0dFVCcsIHVybCwgLyogYXN5bmM6ICovIHRydWUpO1xyXG4gIHhoci5yZXNwb25zZVR5cGUgPSAnYXJyYXlidWZmZXInO1xyXG4gIHhoci5vbmxvYWQgPSBmdW5jdGlvbiB4aHJfb25sb2FkKCkge1xyXG4gICAgaWYgKHhoci5zdGF0dXMgPT0gMjAwIHx8IHhoci5zdGF0dXMgPT0gMCAmJiB4aHIucmVzcG9uc2UpIHtcclxuICAgICAgdmFyIGFzbSA9IG5ldyBVaW50OEFycmF5KHhoci5yZXNwb25zZSk7XHJcbiAgICAgIG9ubG9hZChhc20pO1xyXG4gICAgfSBlbHNlIHtcclxuICAgICAgb25lcnJvcih4aHIpO1xyXG4gICAgfVxyXG4gIH07XHJcbiAgeGhyLm9uZXJyb3IgPSBvbmVycm9yO1xyXG4gIHhoci5zZW5kKG51bGwpO1xyXG59XHJcblxyXG5mdW5jdGlvbiBnZXRBcnJheURhdGFQb2ludGVyPFQ+KGFycmF5OiBTeXN0ZW1fQXJyYXk8VD4pOiBudW1iZXIge1xyXG4gIHJldHVybiA8bnVtYmVyPjxhbnk+YXJyYXkgKyAxMjsgLy8gRmlyc3QgYnl0ZSBmcm9tIGhlcmUgaXMgbGVuZ3RoLCB0aGVuIGZvbGxvd2luZyBieXRlcyBhcmUgZW50cmllc1xyXG59XHJcblxuXG5cbi8vIFdFQlBBQ0sgRk9PVEVSIC8vXG4vLyAuL3NyYy9QbGF0Zm9ybS9Nb25vL01vbm9QbGF0Zm9ybS50cyIsImltcG9ydCB7IGludm9rZVdpdGhKc29uTWFyc2hhbGxpbmcgfSBmcm9tICcuL0ludm9rZVdpdGhKc29uTWFyc2hhbGxpbmcnO1xyXG5pbXBvcnQgeyBhdHRhY2hDb21wb25lbnRUb0VsZW1lbnQsIHJlbmRlckJhdGNoIH0gZnJvbSAnLi4vUmVuZGVyaW5nL1JlbmRlcmVyJztcclxuXHJcbi8qKlxyXG4gKiBUaGUgZGVmaW5pdGl2ZSBsaXN0IG9mIGludGVybmFsIGZ1bmN0aW9ucyBpbnZva2FibGUgZnJvbSAuTkVUIGNvZGUuXHJcbiAqIFRoZXNlIGZ1bmN0aW9uIG5hbWVzIGFyZSB0cmVhdGVkIGFzICdyZXNlcnZlZCcgYW5kIGNhbm5vdCBiZSBwYXNzZWQgdG8gcmVnaXN0ZXJGdW5jdGlvbi5cclxuICovXHJcbmV4cG9ydCBjb25zdCBpbnRlcm5hbFJlZ2lzdGVyZWRGdW5jdGlvbnMgPSB7XHJcbiAgYXR0YWNoQ29tcG9uZW50VG9FbGVtZW50LFxyXG4gIGludm9rZVdpdGhKc29uTWFyc2hhbGxpbmcsXHJcbiAgcmVuZGVyQmF0Y2gsXHJcbn07XHJcblxuXG5cbi8vIFdFQlBBQ0sgRk9PVEVSIC8vXG4vLyAuL3NyYy9JbnRlcm9wL0ludGVybmFsUmVnaXN0ZXJlZEZ1bmN0aW9uLnRzIiwiaW1wb3J0IHsgcGxhdGZvcm0gfSBmcm9tICcuLi9FbnZpcm9ubWVudCc7XHJcbmltcG9ydCB7IFN5c3RlbV9TdHJpbmcgfSBmcm9tICcuLi9QbGF0Zm9ybS9QbGF0Zm9ybSc7XHJcbmltcG9ydCB7IGdldFJlZ2lzdGVyZWRGdW5jdGlvbiB9IGZyb20gJy4vUmVnaXN0ZXJlZEZ1bmN0aW9uJztcclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBpbnZva2VXaXRoSnNvbk1hcnNoYWxsaW5nKGlkZW50aWZpZXI6IFN5c3RlbV9TdHJpbmcsIC4uLmFyZ3NKc29uOiBTeXN0ZW1fU3RyaW5nW10pIHtcclxuICBjb25zdCBpZGVudGlmaWVySnNTdHJpbmcgPSBwbGF0Zm9ybS50b0phdmFTY3JpcHRTdHJpbmcoaWRlbnRpZmllcik7XHJcbiAgY29uc3QgZnVuY0luc3RhbmNlID0gZ2V0UmVnaXN0ZXJlZEZ1bmN0aW9uKGlkZW50aWZpZXJKc1N0cmluZyk7XHJcbiAgY29uc3QgYXJncyA9IGFyZ3NKc29uLm1hcChqc29uID0+IEpTT04ucGFyc2UocGxhdGZvcm0udG9KYXZhU2NyaXB0U3RyaW5nKGpzb24pKSk7XHJcbiAgY29uc3QgcmVzdWx0ID0gZnVuY0luc3RhbmNlLmFwcGx5KG51bGwsIGFyZ3MpO1xyXG4gIGlmIChyZXN1bHQgIT09IG51bGwgJiYgcmVzdWx0ICE9PSB1bmRlZmluZWQpIHtcclxuICAgIGNvbnN0IHJlc3VsdEpzb24gPSBKU09OLnN0cmluZ2lmeShyZXN1bHQpO1xyXG4gICAgcmV0dXJuIHBsYXRmb3JtLnRvRG90TmV0U3RyaW5nKHJlc3VsdEpzb24pO1xyXG4gIH0gZWxzZSB7XHJcbiAgICByZXR1cm4gbnVsbDtcclxuICB9XHJcbn1cclxuXG5cblxuLy8gV0VCUEFDSyBGT09URVIgLy9cbi8vIC4vc3JjL0ludGVyb3AvSW52b2tlV2l0aEpzb25NYXJzaGFsbGluZy50cyIsImltcG9ydCB7IFBvaW50ZXIsIFN5c3RlbV9BcnJheSB9IGZyb20gJy4uL1BsYXRmb3JtL1BsYXRmb3JtJztcclxuaW1wb3J0IHsgcGxhdGZvcm0gfSBmcm9tICcuLi9FbnZpcm9ubWVudCc7XHJcbmltcG9ydCB7IFJlbmRlclRyZWVGcmFtZVBvaW50ZXIgfSBmcm9tICcuL1JlbmRlclRyZWVGcmFtZSc7XHJcbmltcG9ydCB7IFJlbmRlclRyZWVFZGl0UG9pbnRlciB9IGZyb20gJy4vUmVuZGVyVHJlZUVkaXQnO1xyXG5cclxuLy8gS2VlcCBpbiBzeW5jIHdpdGggdGhlIHN0cnVjdHMgaW4gLk5FVCBjb2RlXHJcblxyXG5leHBvcnQgY29uc3QgcmVuZGVyQmF0Y2ggPSB7XHJcbiAgdXBkYXRlZENvbXBvbmVudHM6IChvYmo6IFJlbmRlckJhdGNoUG9pbnRlcikgPT4gcGxhdGZvcm0ucmVhZFN0cnVjdEZpZWxkPEFycmF5UmFuZ2VQb2ludGVyPFJlbmRlclRyZWVEaWZmUG9pbnRlcj4+KG9iaiwgMCksXHJcbiAgcmVmZXJlbmNlRnJhbWVzOiAob2JqOiBSZW5kZXJCYXRjaFBvaW50ZXIpID0+IHBsYXRmb3JtLnJlYWRTdHJ1Y3RGaWVsZDxBcnJheVJhbmdlUG9pbnRlcjxSZW5kZXJUcmVlRnJhbWVQb2ludGVyPj4ob2JqLCBhcnJheVJhbmdlU3RydWN0TGVuZ3RoKSxcclxuICBkaXNwb3NlZENvbXBvbmVudElkczogKG9iajogUmVuZGVyQmF0Y2hQb2ludGVyKSA9PiBwbGF0Zm9ybS5yZWFkU3RydWN0RmllbGQ8QXJyYXlSYW5nZVBvaW50ZXI8bnVtYmVyPj4ob2JqLCBhcnJheVJhbmdlU3RydWN0TGVuZ3RoICsgYXJyYXlSYW5nZVN0cnVjdExlbmd0aCksXHJcbn07XHJcblxyXG5jb25zdCBhcnJheVJhbmdlU3RydWN0TGVuZ3RoID0gODtcclxuZXhwb3J0IGNvbnN0IGFycmF5UmFuZ2UgPSB7XHJcbiAgYXJyYXk6IDxUPihvYmo6IEFycmF5UmFuZ2VQb2ludGVyPFQ+KSA9PiBwbGF0Zm9ybS5yZWFkT2JqZWN0RmllbGQ8U3lzdGVtX0FycmF5PFQ+PihvYmosIDApLFxyXG4gIGNvdW50OiA8VD4ob2JqOiBBcnJheVJhbmdlUG9pbnRlcjxUPikgPT4gcGxhdGZvcm0ucmVhZEludDMyRmllbGQob2JqLCA0KSxcclxufTtcclxuXHJcbmNvbnN0IGFycmF5U2VnbWVudFN0cnVjdExlbmd0aCA9IDEyO1xyXG5leHBvcnQgY29uc3QgYXJyYXlTZWdtZW50ID0ge1xyXG4gIGFycmF5OiA8VD4ob2JqOiBBcnJheVNlZ21lbnRQb2ludGVyPFQ+KSA9PiBwbGF0Zm9ybS5yZWFkT2JqZWN0RmllbGQ8U3lzdGVtX0FycmF5PFQ+PihvYmosIDApLFxyXG4gIG9mZnNldDogPFQ+KG9iajogQXJyYXlTZWdtZW50UG9pbnRlcjxUPikgPT4gcGxhdGZvcm0ucmVhZEludDMyRmllbGQob2JqLCA0KSxcclxuICBjb3VudDogPFQ+KG9iajogQXJyYXlTZWdtZW50UG9pbnRlcjxUPikgPT4gcGxhdGZvcm0ucmVhZEludDMyRmllbGQob2JqLCA4KSxcclxufTtcclxuXHJcbmV4cG9ydCBjb25zdCByZW5kZXJUcmVlRGlmZlN0cnVjdExlbmd0aCA9IDQgKyBhcnJheVNlZ21lbnRTdHJ1Y3RMZW5ndGg7XHJcbmV4cG9ydCBjb25zdCByZW5kZXJUcmVlRGlmZiA9IHtcclxuICBjb21wb25lbnRJZDogKG9iajogUmVuZGVyVHJlZURpZmZQb2ludGVyKSA9PiBwbGF0Zm9ybS5yZWFkSW50MzJGaWVsZChvYmosIDApLFxyXG4gIGVkaXRzOiAob2JqOiBSZW5kZXJUcmVlRGlmZlBvaW50ZXIpID0+IHBsYXRmb3JtLnJlYWRTdHJ1Y3RGaWVsZDxBcnJheVNlZ21lbnRQb2ludGVyPFJlbmRlclRyZWVFZGl0UG9pbnRlcj4+KG9iaiwgNCksICBcclxufTtcclxuXHJcbi8vIE5vbWluYWwgdHlwZXMgdG8gZW5zdXJlIG9ubHkgdmFsaWQgcG9pbnRlcnMgYXJlIHBhc3NlZCB0byB0aGUgZnVuY3Rpb25zIGFib3ZlLlxyXG4vLyBBdCBydW50aW1lIHRoZSB2YWx1ZXMgYXJlIGp1c3QgbnVtYmVycy5cclxuZXhwb3J0IGludGVyZmFjZSBSZW5kZXJCYXRjaFBvaW50ZXIgZXh0ZW5kcyBQb2ludGVyIHsgUmVuZGVyQmF0Y2hQb2ludGVyX19ET19OT1RfSU1QTEVNRU5UOiBhbnkgfVxyXG5leHBvcnQgaW50ZXJmYWNlIEFycmF5UmFuZ2VQb2ludGVyPFQ+IGV4dGVuZHMgUG9pbnRlciB7IEFycmF5UmFuZ2VQb2ludGVyX19ET19OT1RfSU1QTEVNRU5UOiBhbnkgfVxyXG5leHBvcnQgaW50ZXJmYWNlIEFycmF5U2VnbWVudFBvaW50ZXI8VD4gZXh0ZW5kcyBQb2ludGVyIHsgQXJyYXlTZWdtZW50UG9pbnRlcl9fRE9fTk9UX0lNUExFTUVOVDogYW55IH1cclxuZXhwb3J0IGludGVyZmFjZSBSZW5kZXJUcmVlRGlmZlBvaW50ZXIgZXh0ZW5kcyBQb2ludGVyIHsgUmVuZGVyVHJlZURpZmZQb2ludGVyX19ET19OT1RfSU1QTEVNRU5UOiBhbnkgfVxyXG5cblxuXG4vLyBXRUJQQUNLIEZPT1RFUiAvL1xuLy8gLi9zcmMvUmVuZGVyaW5nL1JlbmRlckJhdGNoLnRzIiwiaW1wb3J0IHsgU3lzdGVtX0FycmF5LCBNZXRob2RIYW5kbGUgfSBmcm9tICcuLi9QbGF0Zm9ybS9QbGF0Zm9ybSc7XHJcbmltcG9ydCB7IGdldFJlbmRlclRyZWVFZGl0UHRyLCByZW5kZXJUcmVlRWRpdCwgUmVuZGVyVHJlZUVkaXRQb2ludGVyLCBFZGl0VHlwZSB9IGZyb20gJy4vUmVuZGVyVHJlZUVkaXQnO1xyXG5pbXBvcnQgeyBnZXRUcmVlRnJhbWVQdHIsIHJlbmRlclRyZWVGcmFtZSwgRnJhbWVUeXBlLCBSZW5kZXJUcmVlRnJhbWVQb2ludGVyIH0gZnJvbSAnLi9SZW5kZXJUcmVlRnJhbWUnO1xyXG5pbXBvcnQgeyBwbGF0Zm9ybSB9IGZyb20gJy4uL0Vudmlyb25tZW50JztcclxubGV0IHJhaXNlRXZlbnRNZXRob2Q6IE1ldGhvZEhhbmRsZTtcclxubGV0IHJlbmRlckNvbXBvbmVudE1ldGhvZDogTWV0aG9kSGFuZGxlO1xyXG5cclxuZXhwb3J0IGNsYXNzIEJyb3dzZXJSZW5kZXJlciB7XHJcbiAgcHJpdmF0ZSBjaGlsZENvbXBvbmVudExvY2F0aW9uczogeyBbY29tcG9uZW50SWQ6IG51bWJlcl06IEVsZW1lbnQgfSA9IHt9O1xyXG5cclxuICBjb25zdHJ1Y3Rvcihwcml2YXRlIGJyb3dzZXJSZW5kZXJlcklkOiBudW1iZXIpIHtcclxuICB9XHJcblxyXG4gIHB1YmxpYyBhdHRhY2hDb21wb25lbnRUb0VsZW1lbnQoY29tcG9uZW50SWQ6IG51bWJlciwgZWxlbWVudDogRWxlbWVudCkge1xyXG4gICAgdGhpcy5jaGlsZENvbXBvbmVudExvY2F0aW9uc1tjb21wb25lbnRJZF0gPSBlbGVtZW50O1xyXG4gIH1cclxuXHJcbiAgcHVibGljIHVwZGF0ZUNvbXBvbmVudChjb21wb25lbnRJZDogbnVtYmVyLCBlZGl0czogU3lzdGVtX0FycmF5PFJlbmRlclRyZWVFZGl0UG9pbnRlcj4sIGVkaXRzT2Zmc2V0OiBudW1iZXIsIGVkaXRzTGVuZ3RoOiBudW1iZXIsIHJlZmVyZW5jZUZyYW1lczogU3lzdGVtX0FycmF5PFJlbmRlclRyZWVGcmFtZVBvaW50ZXI+KSB7XHJcbiAgICBjb25zdCBlbGVtZW50ID0gdGhpcy5jaGlsZENvbXBvbmVudExvY2F0aW9uc1tjb21wb25lbnRJZF07XHJcbiAgICBpZiAoIWVsZW1lbnQpIHtcclxuICAgICAgdGhyb3cgbmV3IEVycm9yKGBObyBlbGVtZW50IGlzIGN1cnJlbnRseSBhc3NvY2lhdGVkIHdpdGggY29tcG9uZW50ICR7Y29tcG9uZW50SWR9YCk7XHJcbiAgICB9XHJcblxyXG4gICAgdGhpcy5hcHBseUVkaXRzKGNvbXBvbmVudElkLCBlbGVtZW50LCAwLCBlZGl0cywgZWRpdHNPZmZzZXQsIGVkaXRzTGVuZ3RoLCByZWZlcmVuY2VGcmFtZXMpO1xyXG4gIH1cclxuXHJcbiAgcHVibGljIGRpc3Bvc2VDb21wb25lbnQoY29tcG9uZW50SWQ6IG51bWJlcikge1xyXG4gICAgZGVsZXRlIHRoaXMuY2hpbGRDb21wb25lbnRMb2NhdGlvbnNbY29tcG9uZW50SWRdO1xyXG4gIH1cclxuXHJcbiAgYXBwbHlFZGl0cyhjb21wb25lbnRJZDogbnVtYmVyLCBwYXJlbnQ6IEVsZW1lbnQsIGNoaWxkSW5kZXg6IG51bWJlciwgZWRpdHM6IFN5c3RlbV9BcnJheTxSZW5kZXJUcmVlRWRpdFBvaW50ZXI+LCBlZGl0c09mZnNldDogbnVtYmVyLCBlZGl0c0xlbmd0aDogbnVtYmVyLCByZWZlcmVuY2VGcmFtZXM6IFN5c3RlbV9BcnJheTxSZW5kZXJUcmVlRnJhbWVQb2ludGVyPikge1xyXG4gICAgbGV0IGN1cnJlbnREZXB0aCA9IDA7XHJcbiAgICBsZXQgY2hpbGRJbmRleEF0Q3VycmVudERlcHRoID0gY2hpbGRJbmRleDtcclxuICAgIGNvbnN0IG1heEVkaXRJbmRleEV4Y2wgPSBlZGl0c09mZnNldCArIGVkaXRzTGVuZ3RoO1xyXG4gICAgZm9yIChsZXQgZWRpdEluZGV4ID0gZWRpdHNPZmZzZXQ7IGVkaXRJbmRleCA8IG1heEVkaXRJbmRleEV4Y2w7IGVkaXRJbmRleCsrKSB7XHJcbiAgICAgIGNvbnN0IGVkaXQgPSBnZXRSZW5kZXJUcmVlRWRpdFB0cihlZGl0cywgZWRpdEluZGV4KTtcclxuICAgICAgY29uc3QgZWRpdFR5cGUgPSByZW5kZXJUcmVlRWRpdC50eXBlKGVkaXQpO1xyXG4gICAgICBzd2l0Y2ggKGVkaXRUeXBlKSB7XHJcbiAgICAgICAgY2FzZSBFZGl0VHlwZS5wcmVwZW5kRnJhbWU6IHtcclxuICAgICAgICAgIGNvbnN0IGZyYW1lSW5kZXggPSByZW5kZXJUcmVlRWRpdC5uZXdUcmVlSW5kZXgoZWRpdCk7XHJcbiAgICAgICAgICBjb25zdCBmcmFtZSA9IGdldFRyZWVGcmFtZVB0cihyZWZlcmVuY2VGcmFtZXMsIGZyYW1lSW5kZXgpO1xyXG4gICAgICAgICAgY29uc3Qgc2libGluZ0luZGV4ID0gcmVuZGVyVHJlZUVkaXQuc2libGluZ0luZGV4KGVkaXQpO1xyXG4gICAgICAgICAgdGhpcy5pbnNlcnRGcmFtZShjb21wb25lbnRJZCwgcGFyZW50LCBjaGlsZEluZGV4QXRDdXJyZW50RGVwdGggKyBzaWJsaW5nSW5kZXgsIHJlZmVyZW5jZUZyYW1lcywgZnJhbWUsIGZyYW1lSW5kZXgpO1xyXG4gICAgICAgICAgYnJlYWs7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGNhc2UgRWRpdFR5cGUucmVtb3ZlRnJhbWU6IHtcclxuICAgICAgICAgIGNvbnN0IHNpYmxpbmdJbmRleCA9IHJlbmRlclRyZWVFZGl0LnNpYmxpbmdJbmRleChlZGl0KTtcclxuICAgICAgICAgIHJlbW92ZU5vZGVGcm9tRE9NKHBhcmVudCwgY2hpbGRJbmRleEF0Q3VycmVudERlcHRoICsgc2libGluZ0luZGV4KTtcclxuICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgIH1cclxuICAgICAgICBjYXNlIEVkaXRUeXBlLnNldEF0dHJpYnV0ZToge1xyXG4gICAgICAgICAgY29uc3QgZnJhbWVJbmRleCA9IHJlbmRlclRyZWVFZGl0Lm5ld1RyZWVJbmRleChlZGl0KTtcclxuICAgICAgICAgIGNvbnN0IGZyYW1lID0gZ2V0VHJlZUZyYW1lUHRyKHJlZmVyZW5jZUZyYW1lcywgZnJhbWVJbmRleCk7XHJcbiAgICAgICAgICBjb25zdCBzaWJsaW5nSW5kZXggPSByZW5kZXJUcmVlRWRpdC5zaWJsaW5nSW5kZXgoZWRpdCk7XHJcbiAgICAgICAgICBjb25zdCBlbGVtZW50ID0gcGFyZW50LmNoaWxkTm9kZXNbY2hpbGRJbmRleEF0Q3VycmVudERlcHRoICsgc2libGluZ0luZGV4XSBhcyBIVE1MRWxlbWVudDtcclxuICAgICAgICAgIHRoaXMuYXBwbHlBdHRyaWJ1dGUoY29tcG9uZW50SWQsIGVsZW1lbnQsIGZyYW1lKTtcclxuICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgIH1cclxuICAgICAgICBjYXNlIEVkaXRUeXBlLnJlbW92ZUF0dHJpYnV0ZToge1xyXG4gICAgICAgICAgY29uc3Qgc2libGluZ0luZGV4ID0gcmVuZGVyVHJlZUVkaXQuc2libGluZ0luZGV4KGVkaXQpO1xyXG4gICAgICAgICAgcmVtb3ZlQXR0cmlidXRlRnJvbURPTShwYXJlbnQsIGNoaWxkSW5kZXhBdEN1cnJlbnREZXB0aCArIHNpYmxpbmdJbmRleCwgcmVuZGVyVHJlZUVkaXQucmVtb3ZlZEF0dHJpYnV0ZU5hbWUoZWRpdCkhKTtcclxuICAgICAgICAgIGJyZWFrO1xyXG4gICAgICAgIH1cclxuICAgICAgICBjYXNlIEVkaXRUeXBlLnVwZGF0ZVRleHQ6IHtcclxuICAgICAgICAgIGNvbnN0IGZyYW1lSW5kZXggPSByZW5kZXJUcmVlRWRpdC5uZXdUcmVlSW5kZXgoZWRpdCk7XHJcbiAgICAgICAgICBjb25zdCBmcmFtZSA9IGdldFRyZWVGcmFtZVB0cihyZWZlcmVuY2VGcmFtZXMsIGZyYW1lSW5kZXgpO1xyXG4gICAgICAgICAgY29uc3Qgc2libGluZ0luZGV4ID0gcmVuZGVyVHJlZUVkaXQuc2libGluZ0luZGV4KGVkaXQpO1xyXG4gICAgICAgICAgY29uc3QgZG9tVGV4dE5vZGUgPSBwYXJlbnQuY2hpbGROb2Rlc1tjaGlsZEluZGV4QXRDdXJyZW50RGVwdGggKyBzaWJsaW5nSW5kZXhdIGFzIFRleHQ7XHJcbiAgICAgICAgICBkb21UZXh0Tm9kZS50ZXh0Q29udGVudCA9IHJlbmRlclRyZWVGcmFtZS50ZXh0Q29udGVudChmcmFtZSk7XHJcbiAgICAgICAgICBicmVhaztcclxuICAgICAgICB9XHJcbiAgICAgICAgY2FzZSBFZGl0VHlwZS5zdGVwSW46IHtcclxuICAgICAgICAgIGNvbnN0IHNpYmxpbmdJbmRleCA9IHJlbmRlclRyZWVFZGl0LnNpYmxpbmdJbmRleChlZGl0KTtcclxuICAgICAgICAgIHBhcmVudCA9IHBhcmVudC5jaGlsZE5vZGVzW2NoaWxkSW5kZXhBdEN1cnJlbnREZXB0aCArIHNpYmxpbmdJbmRleF0gYXMgSFRNTEVsZW1lbnQ7XHJcbiAgICAgICAgICBjdXJyZW50RGVwdGgrKztcclxuICAgICAgICAgIGNoaWxkSW5kZXhBdEN1cnJlbnREZXB0aCA9IDA7XHJcbiAgICAgICAgICBicmVhaztcclxuICAgICAgICB9XHJcbiAgICAgICAgY2FzZSBFZGl0VHlwZS5zdGVwT3V0OiB7XHJcbiAgICAgICAgICBwYXJlbnQgPSBwYXJlbnQucGFyZW50RWxlbWVudCE7XHJcbiAgICAgICAgICBjdXJyZW50RGVwdGgtLTtcclxuICAgICAgICAgIGNoaWxkSW5kZXhBdEN1cnJlbnREZXB0aCA9IGN1cnJlbnREZXB0aCA9PT0gMCA/IGNoaWxkSW5kZXggOiAwOyAvLyBUaGUgY2hpbGRJbmRleCBpcyBvbmx5IGV2ZXIgbm9uemVybyBhdCB6ZXJvIGRlcHRoXHJcbiAgICAgICAgICBicmVhaztcclxuICAgICAgICB9XHJcbiAgICAgICAgZGVmYXVsdDoge1xyXG4gICAgICAgICAgY29uc3QgdW5rbm93blR5cGU6IG5ldmVyID0gZWRpdFR5cGU7IC8vIENvbXBpbGUtdGltZSB2ZXJpZmljYXRpb24gdGhhdCB0aGUgc3dpdGNoIHdhcyBleGhhdXN0aXZlXHJcbiAgICAgICAgICB0aHJvdyBuZXcgRXJyb3IoYFVua25vd24gZWRpdCB0eXBlOiAke3Vua25vd25UeXBlfWApO1xyXG4gICAgICAgIH1cclxuICAgICAgfVxyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgaW5zZXJ0RnJhbWUoY29tcG9uZW50SWQ6IG51bWJlciwgcGFyZW50OiBFbGVtZW50LCBjaGlsZEluZGV4OiBudW1iZXIsIGZyYW1lczogU3lzdGVtX0FycmF5PFJlbmRlclRyZWVGcmFtZVBvaW50ZXI+LCBmcmFtZTogUmVuZGVyVHJlZUZyYW1lUG9pbnRlciwgZnJhbWVJbmRleDogbnVtYmVyKTogbnVtYmVyIHtcclxuICAgIGNvbnN0IGZyYW1lVHlwZSA9IHJlbmRlclRyZWVGcmFtZS5mcmFtZVR5cGUoZnJhbWUpO1xyXG4gICAgc3dpdGNoIChmcmFtZVR5cGUpIHtcclxuICAgICAgY2FzZSBGcmFtZVR5cGUuZWxlbWVudDpcclxuICAgICAgICB0aGlzLmluc2VydEVsZW1lbnQoY29tcG9uZW50SWQsIHBhcmVudCwgY2hpbGRJbmRleCwgZnJhbWVzLCBmcmFtZSwgZnJhbWVJbmRleCk7XHJcbiAgICAgICAgcmV0dXJuIDE7XHJcbiAgICAgIGNhc2UgRnJhbWVUeXBlLnRleHQ6XHJcbiAgICAgICAgdGhpcy5pbnNlcnRUZXh0KHBhcmVudCwgY2hpbGRJbmRleCwgZnJhbWUpO1xyXG4gICAgICAgIHJldHVybiAxO1xyXG4gICAgICBjYXNlIEZyYW1lVHlwZS5hdHRyaWJ1dGU6XHJcbiAgICAgICAgdGhyb3cgbmV3IEVycm9yKCdBdHRyaWJ1dGUgZnJhbWVzIHNob3VsZCBvbmx5IGJlIHByZXNlbnQgYXMgbGVhZGluZyBjaGlsZHJlbiBvZiBlbGVtZW50IGZyYW1lcy4nKTtcclxuICAgICAgY2FzZSBGcmFtZVR5cGUuY29tcG9uZW50OlxyXG4gICAgICAgIHRoaXMuaW5zZXJ0Q29tcG9uZW50KHBhcmVudCwgY2hpbGRJbmRleCwgZnJhbWUpO1xyXG4gICAgICAgIHJldHVybiAxO1xyXG4gICAgICBjYXNlIEZyYW1lVHlwZS5yZWdpb246XHJcbiAgICAgICAgcmV0dXJuIHRoaXMuaW5zZXJ0RnJhbWVSYW5nZShjb21wb25lbnRJZCwgcGFyZW50LCBjaGlsZEluZGV4LCBmcmFtZXMsIGZyYW1lSW5kZXggKyAxLCBmcmFtZUluZGV4ICsgcmVuZGVyVHJlZUZyYW1lLnN1YnRyZWVMZW5ndGgoZnJhbWUpKTtcclxuICAgICAgZGVmYXVsdDpcclxuICAgICAgICBjb25zdCB1bmtub3duVHlwZTogbmV2ZXIgPSBmcmFtZVR5cGU7IC8vIENvbXBpbGUtdGltZSB2ZXJpZmljYXRpb24gdGhhdCB0aGUgc3dpdGNoIHdhcyBleGhhdXN0aXZlXHJcbiAgICAgICAgdGhyb3cgbmV3IEVycm9yKGBVbmtub3duIGZyYW1lIHR5cGU6ICR7dW5rbm93blR5cGV9YCk7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICBpbnNlcnRFbGVtZW50KGNvbXBvbmVudElkOiBudW1iZXIsIHBhcmVudDogRWxlbWVudCwgY2hpbGRJbmRleDogbnVtYmVyLCBmcmFtZXM6IFN5c3RlbV9BcnJheTxSZW5kZXJUcmVlRnJhbWVQb2ludGVyPiwgZnJhbWU6IFJlbmRlclRyZWVGcmFtZVBvaW50ZXIsIGZyYW1lSW5kZXg6IG51bWJlcikge1xyXG4gICAgY29uc3QgdGFnTmFtZSA9IHJlbmRlclRyZWVGcmFtZS5lbGVtZW50TmFtZShmcmFtZSkhO1xyXG4gICAgY29uc3QgbmV3RG9tRWxlbWVudCA9IGRvY3VtZW50LmNyZWF0ZUVsZW1lbnQodGFnTmFtZSk7XHJcbiAgICBpbnNlcnROb2RlSW50b0RPTShuZXdEb21FbGVtZW50LCBwYXJlbnQsIGNoaWxkSW5kZXgpO1xyXG5cclxuICAgIC8vIEFwcGx5IGF0dHJpYnV0ZXNcclxuICAgIGNvbnN0IGRlc2NlbmRhbnRzRW5kSW5kZXhFeGNsID0gZnJhbWVJbmRleCArIHJlbmRlclRyZWVGcmFtZS5zdWJ0cmVlTGVuZ3RoKGZyYW1lKTtcclxuICAgIGZvciAobGV0IGRlc2NlbmRhbnRJbmRleCA9IGZyYW1lSW5kZXggKyAxOyBkZXNjZW5kYW50SW5kZXggPCBkZXNjZW5kYW50c0VuZEluZGV4RXhjbDsgZGVzY2VuZGFudEluZGV4KyspIHtcclxuICAgICAgY29uc3QgZGVzY2VuZGFudEZyYW1lID0gZ2V0VHJlZUZyYW1lUHRyKGZyYW1lcywgZGVzY2VuZGFudEluZGV4KTtcclxuICAgICAgaWYgKHJlbmRlclRyZWVGcmFtZS5mcmFtZVR5cGUoZGVzY2VuZGFudEZyYW1lKSA9PT0gRnJhbWVUeXBlLmF0dHJpYnV0ZSkge1xyXG4gICAgICAgIHRoaXMuYXBwbHlBdHRyaWJ1dGUoY29tcG9uZW50SWQsIG5ld0RvbUVsZW1lbnQsIGRlc2NlbmRhbnRGcmFtZSk7XHJcbiAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgLy8gQXMgc29vbiBhcyB3ZSBzZWUgYSBub24tYXR0cmlidXRlIGNoaWxkLCBhbGwgdGhlIHN1YnNlcXVlbnQgY2hpbGQgZnJhbWVzIGFyZVxyXG4gICAgICAgIC8vIG5vdCBhdHRyaWJ1dGVzLCBzbyBiYWlsIG91dCBhbmQgaW5zZXJ0IHRoZSByZW1uYW50cyByZWN1cnNpdmVseVxyXG4gICAgICAgIHRoaXMuaW5zZXJ0RnJhbWVSYW5nZShjb21wb25lbnRJZCwgbmV3RG9tRWxlbWVudCwgMCwgZnJhbWVzLCBkZXNjZW5kYW50SW5kZXgsIGRlc2NlbmRhbnRzRW5kSW5kZXhFeGNsKTtcclxuICAgICAgICBicmVhaztcclxuICAgICAgfVxyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgaW5zZXJ0Q29tcG9uZW50KHBhcmVudDogRWxlbWVudCwgY2hpbGRJbmRleDogbnVtYmVyLCBmcmFtZTogUmVuZGVyVHJlZUZyYW1lUG9pbnRlcikge1xyXG4gICAgLy8gQ3VycmVudGx5LCB0byBzdXBwb3J0IE8oMSkgbG9va3VwcyBmcm9tIHJlbmRlciB0cmVlIGZyYW1lcyB0byBET00gbm9kZXMsIHdlIHJlbHkgb25cclxuICAgIC8vIGVhY2ggY2hpbGQgY29tcG9uZW50IGV4aXN0aW5nIGFzIGEgc2luZ2xlIHRvcC1sZXZlbCBlbGVtZW50IGluIHRoZSBET00uIFRvIGd1YXJhbnRlZVxyXG4gICAgLy8gdGhhdCwgd2Ugd3JhcCBjaGlsZCBjb21wb25lbnRzIGluIHRoZXNlICdibGF6b3ItY29tcG9uZW50JyB3cmFwcGVycy5cclxuICAgIC8vIFRvIGltcHJvdmUgb24gdGhpcyBpbiB0aGUgZnV0dXJlOlxyXG4gICAgLy8gLSBJZiB3ZSBjYW4gc3RhdGljYWxseSBkZXRlY3QgdGhhdCBhIGdpdmVuIGNvbXBvbmVudCBhbHdheXMgcHJvZHVjZXMgYSBzaW5nbGUgdG9wLWxldmVsXHJcbiAgICAvLyAgIGVsZW1lbnQgYW55d2F5LCB0aGVuIGRvbid0IHdyYXAgaXQgaW4gYSBmdXJ0aGVyIG5vbnN0YW5kYXJkIGVsZW1lbnRcclxuICAgIC8vIC0gSWYgd2UgcmVhbGx5IHdhbnQgdG8gc3VwcG9ydCBjaGlsZCBjb21wb25lbnRzIHByb2R1Y2luZyBtdWx0aXBsZSB0b3AtbGV2ZWwgZnJhbWVzIGFuZFxyXG4gICAgLy8gICBub3QgYmVpbmcgd3JhcHBlZCBpbiBhIGNvbnRhaW5lciBhdCBhbGwsIHRoZW4gZXZlcnkgdGltZSBhIGNvbXBvbmVudCBpcyByZWZyZXNoZWQgaW5cclxuICAgIC8vICAgdGhlIERPTSwgd2UgY291bGQgdXBkYXRlIGFuIGFycmF5IG9uIHRoZSBwYXJlbnQgZWxlbWVudCB0aGF0IHNwZWNpZmllcyBob3cgbWFueSBET01cclxuICAgIC8vICAgbm9kZXMgY29ycmVzcG9uZCB0byBlYWNoIG9mIGl0cyByZW5kZXIgdHJlZSBmcmFtZXMuIFRoZW4gd2hlbiB0aGF0IHBhcmVudCB3YW50cyB0b1xyXG4gICAgLy8gICBsb2NhdGUgdGhlIGZpcnN0IERPTSBub2RlIGZvciBhIHJlbmRlciB0cmVlIGZyYW1lLCBpdCBjYW4gc3VtIGFsbCB0aGUgZnJhbWUgY291bnRzIGZvclxyXG4gICAgLy8gICBhbGwgdGhlIHByZWNlZGluZyByZW5kZXIgdHJlZXMgZnJhbWVzLiBJdCdzIE8oTiksIGJ1dCB3aGVyZSBOIGlzIHRoZSBudW1iZXIgb2Ygc2libGluZ3NcclxuICAgIC8vICAgKGNvdW50aW5nIGNoaWxkIGNvbXBvbmVudHMgYXMgYSBzaW5nbGUgaXRlbSksIHNvIE4gd2lsbCByYXJlbHkgaWYgZXZlciBiZSBsYXJnZS5cclxuICAgIC8vICAgV2UgY291bGQgZXZlbiBrZWVwIHRyYWNrIG9mIHdoZXRoZXIgYWxsIHRoZSBjaGlsZCBjb21wb25lbnRzIGhhcHBlbiB0byBoYXZlIGV4YWN0bHkgMVxyXG4gICAgLy8gICB0b3AgbGV2ZWwgZnJhbWVzLCBhbmQgaW4gdGhhdCBjYXNlLCB0aGVyZSdzIG5vIG5lZWQgdG8gc3VtIGFzIHdlIGNhbiBkbyBkaXJlY3QgbG9va3Vwcy5cclxuICAgIGNvbnN0IGNvbnRhaW5lckVsZW1lbnQgPSBkb2N1bWVudC5jcmVhdGVFbGVtZW50KCdibGF6b3ItY29tcG9uZW50Jyk7XHJcbiAgICBpbnNlcnROb2RlSW50b0RPTShjb250YWluZXJFbGVtZW50LCBwYXJlbnQsIGNoaWxkSW5kZXgpO1xyXG5cclxuICAgIC8vIEFsbCB3ZSBoYXZlIHRvIGRvIGlzIGFzc29jaWF0ZSB0aGUgY2hpbGQgY29tcG9uZW50IElEIHdpdGggaXRzIGxvY2F0aW9uLiBXZSBkb24ndCBhY3R1YWxseVxyXG4gICAgLy8gZG8gYW55IHJlbmRlcmluZyBoZXJlLCBiZWNhdXNlIHRoZSBkaWZmIGZvciB0aGUgY2hpbGQgd2lsbCBhcHBlYXIgbGF0ZXIgaW4gdGhlIHJlbmRlciBiYXRjaC5cclxuICAgIGNvbnN0IGNoaWxkQ29tcG9uZW50SWQgPSByZW5kZXJUcmVlRnJhbWUuY29tcG9uZW50SWQoZnJhbWUpO1xyXG4gICAgdGhpcy5hdHRhY2hDb21wb25lbnRUb0VsZW1lbnQoY2hpbGRDb21wb25lbnRJZCwgY29udGFpbmVyRWxlbWVudCk7XHJcbiAgfVxyXG5cclxuICBpbnNlcnRUZXh0KHBhcmVudDogRWxlbWVudCwgY2hpbGRJbmRleDogbnVtYmVyLCB0ZXh0RnJhbWU6IFJlbmRlclRyZWVGcmFtZVBvaW50ZXIpIHtcclxuICAgIGNvbnN0IHRleHRDb250ZW50ID0gcmVuZGVyVHJlZUZyYW1lLnRleHRDb250ZW50KHRleHRGcmFtZSkhO1xyXG4gICAgY29uc3QgbmV3RG9tVGV4dE5vZGUgPSBkb2N1bWVudC5jcmVhdGVUZXh0Tm9kZSh0ZXh0Q29udGVudCk7XHJcbiAgICBpbnNlcnROb2RlSW50b0RPTShuZXdEb21UZXh0Tm9kZSwgcGFyZW50LCBjaGlsZEluZGV4KTtcclxuICB9XHJcblxyXG4gIGFwcGx5QXR0cmlidXRlKGNvbXBvbmVudElkOiBudW1iZXIsIHRvRG9tRWxlbWVudDogRWxlbWVudCwgYXR0cmlidXRlRnJhbWU6IFJlbmRlclRyZWVGcmFtZVBvaW50ZXIpIHtcclxuICAgIGNvbnN0IGF0dHJpYnV0ZU5hbWUgPSByZW5kZXJUcmVlRnJhbWUuYXR0cmlidXRlTmFtZShhdHRyaWJ1dGVGcmFtZSkhO1xyXG4gICAgY29uc3QgYnJvd3NlclJlbmRlcmVySWQgPSB0aGlzLmJyb3dzZXJSZW5kZXJlcklkO1xyXG4gICAgY29uc3QgZXZlbnRIYW5kbGVySWQgPSByZW5kZXJUcmVlRnJhbWUuYXR0cmlidXRlRXZlbnRIYW5kbGVySWQoYXR0cmlidXRlRnJhbWUpO1xyXG5cclxuICAgIGlmIChhdHRyaWJ1dGVOYW1lID09PSAndmFsdWUnKSB7XHJcbiAgICAgIGlmICh0aGlzLnRyeUFwcGx5VmFsdWVQcm9wZXJ0eSh0b0RvbUVsZW1lbnQsIHJlbmRlclRyZWVGcmFtZS5hdHRyaWJ1dGVWYWx1ZShhdHRyaWJ1dGVGcmFtZSkpKSB7XHJcbiAgICAgICAgcmV0dXJuOyAvLyBJZiB0aGlzIERPTSBlbGVtZW50IHR5cGUgaGFzIHNwZWNpYWwgJ3ZhbHVlJyBoYW5kbGluZywgZG9uJ3QgYWxzbyB3cml0ZSBpdCBhcyBhbiBhdHRyaWJ1dGVcclxuICAgICAgfVxyXG4gICAgfVxyXG5cclxuICAgIC8vIFRPRE86IEluc3RlYWQgb2YgYXBwbHlpbmcgc2VwYXJhdGUgZXZlbnQgbGlzdGVuZXJzIHRvIGVhY2ggRE9NIGVsZW1lbnQsIHVzZSBldmVudCBkZWxlZ2F0aW9uXHJcbiAgICAvLyBhbmQgcmVtb3ZlIGFsbCB0aGUgX2JsYXpvcipMaXN0ZW5lciBoYWNrc1xyXG4gICAgc3dpdGNoIChhdHRyaWJ1dGVOYW1lKSB7XHJcbiAgICAgIGNhc2UgJ29uY2xpY2snOiB7XHJcbiAgICAgICAgdG9Eb21FbGVtZW50LnJlbW92ZUV2ZW50TGlzdGVuZXIoJ2NsaWNrJywgdG9Eb21FbGVtZW50WydfYmxhem9yQ2xpY2tMaXN0ZW5lciddKTtcclxuICAgICAgICBjb25zdCBsaXN0ZW5lciA9IGV2dCA9PiByYWlzZUV2ZW50KGV2dCwgYnJvd3NlclJlbmRlcmVySWQsIGNvbXBvbmVudElkLCBldmVudEhhbmRsZXJJZCwgJ21vdXNlJywgeyBUeXBlOiAnY2xpY2snIH0pO1xyXG4gICAgICAgIHRvRG9tRWxlbWVudFsnX2JsYXpvckNsaWNrTGlzdGVuZXInXSA9IGxpc3RlbmVyO1xyXG4gICAgICAgIHRvRG9tRWxlbWVudC5hZGRFdmVudExpc3RlbmVyKCdjbGljaycsIGxpc3RlbmVyKTtcclxuICAgICAgICBicmVhaztcclxuICAgICAgfVxyXG4gICAgICBjYXNlICdvbmNoYW5nZSc6IHtcclxuICAgICAgICB0b0RvbUVsZW1lbnQucmVtb3ZlRXZlbnRMaXN0ZW5lcignY2hhbmdlJywgdG9Eb21FbGVtZW50WydfYmxhem9yQ2hhbmdlTGlzdGVuZXInXSk7XHJcbiAgICAgICAgY29uc3QgdGFyZ2V0SXNDaGVja2JveCA9IGlzQ2hlY2tib3godG9Eb21FbGVtZW50KTtcclxuICAgICAgICBjb25zdCBsaXN0ZW5lciA9IGV2dCA9PiB7XHJcbiAgICAgICAgICBjb25zdCBuZXdWYWx1ZSA9IHRhcmdldElzQ2hlY2tib3ggPyBldnQudGFyZ2V0LmNoZWNrZWQgOiBldnQudGFyZ2V0LnZhbHVlO1xyXG4gICAgICAgICAgcmFpc2VFdmVudChldnQsIGJyb3dzZXJSZW5kZXJlcklkLCBjb21wb25lbnRJZCwgZXZlbnRIYW5kbGVySWQsICdjaGFuZ2UnLCB7IFR5cGU6ICdjaGFuZ2UnLCBWYWx1ZTogbmV3VmFsdWUgfSk7XHJcbiAgICAgICAgfTtcclxuICAgICAgICB0b0RvbUVsZW1lbnRbJ19ibGF6b3JDaGFuZ2VMaXN0ZW5lciddID0gbGlzdGVuZXI7XHJcbiAgICAgICAgdG9Eb21FbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoJ2NoYW5nZScsIGxpc3RlbmVyKTtcclxuICAgICAgICBicmVhaztcclxuICAgICAgfVxyXG4gICAgICBjYXNlICdvbmtleXByZXNzJzoge1xyXG4gICAgICAgIHRvRG9tRWxlbWVudC5yZW1vdmVFdmVudExpc3RlbmVyKCdrZXlwcmVzcycsIHRvRG9tRWxlbWVudFsnX2JsYXpvcktleXByZXNzTGlzdGVuZXInXSk7XHJcbiAgICAgICAgY29uc3QgbGlzdGVuZXIgPSBldnQgPT4ge1xyXG4gICAgICAgICAgLy8gVGhpcyBkb2VzIG5vdCBhY2NvdW50IGZvciBzcGVjaWFsIGtleXMgbm9yIGNyb3NzLWJyb3dzZXIgZGlmZmVyZW5jZXMuIFNvIGZhciBpdCdzXHJcbiAgICAgICAgICAvLyBqdXN0IHRvIGVzdGFibGlzaCB0aGF0IHdlIGNhbiBwYXNzIHBhcmFtZXRlcnMgd2hlbiByYWlzaW5nIGV2ZW50cy5cclxuICAgICAgICAgIC8vIFdlIHVzZSBDIy1zdHlsZSBQYXNjYWxDYXNlIG9uIHRoZSBldmVudEluZm8gdG8gc2ltcGxpZnkgZGVzZXJpYWxpemF0aW9uLCBidXQgdGhpcyBjb3VsZFxyXG4gICAgICAgICAgLy8gY2hhbmdlIGlmIHdlIGludHJvZHVjZWQgYSByaWNoZXIgSlNPTiBsaWJyYXJ5IG9uIHRoZSAuTkVUIHNpZGUuXHJcbiAgICAgICAgICByYWlzZUV2ZW50KGV2dCwgYnJvd3NlclJlbmRlcmVySWQsIGNvbXBvbmVudElkLCBldmVudEhhbmRsZXJJZCwgJ2tleWJvYXJkJywgeyBUeXBlOiBldnQudHlwZSwgS2V5OiAoZXZ0IGFzIGFueSkua2V5IH0pO1xyXG4gICAgICAgIH07XHJcbiAgICAgICAgdG9Eb21FbGVtZW50WydfYmxhem9yS2V5cHJlc3NMaXN0ZW5lciddID0gbGlzdGVuZXI7XHJcbiAgICAgICAgdG9Eb21FbGVtZW50LmFkZEV2ZW50TGlzdGVuZXIoJ2tleXByZXNzJywgbGlzdGVuZXIpO1xyXG4gICAgICAgIGJyZWFrO1xyXG4gICAgICB9XHJcbiAgICAgIGRlZmF1bHQ6XHJcbiAgICAgICAgLy8gVHJlYXQgYXMgYSByZWd1bGFyIHN0cmluZy12YWx1ZWQgYXR0cmlidXRlXHJcbiAgICAgICAgdG9Eb21FbGVtZW50LnNldEF0dHJpYnV0ZShcclxuICAgICAgICAgIGF0dHJpYnV0ZU5hbWUsXHJcbiAgICAgICAgICByZW5kZXJUcmVlRnJhbWUuYXR0cmlidXRlVmFsdWUoYXR0cmlidXRlRnJhbWUpIVxyXG4gICAgICAgICk7XHJcbiAgICAgICAgYnJlYWs7XHJcbiAgICB9XHJcbiAgfVxyXG5cclxuICB0cnlBcHBseVZhbHVlUHJvcGVydHkoZWxlbWVudDogRWxlbWVudCwgdmFsdWU6IHN0cmluZyB8IG51bGwpIHtcclxuICAgIC8vIENlcnRhaW4gZWxlbWVudHMgaGF2ZSBidWlsdC1pbiBiZWhhdmlvdXIgZm9yIHRoZWlyICd2YWx1ZScgcHJvcGVydHlcclxuICAgIHN3aXRjaCAoZWxlbWVudC50YWdOYW1lKSB7XHJcbiAgICAgIGNhc2UgJ0lOUFVUJzpcclxuICAgICAgY2FzZSAnU0VMRUNUJzpcclxuICAgICAgICBpZiAoaXNDaGVja2JveChlbGVtZW50KSkge1xyXG4gICAgICAgICAgKGVsZW1lbnQgYXMgSFRNTElucHV0RWxlbWVudCkuY2hlY2tlZCA9IHZhbHVlID09PSAnVHJ1ZSc7XHJcbiAgICAgICAgfSBlbHNlIHtcclxuICAgICAgICAgIC8vIE5vdGU6IHRoaXMgZG9lbid0IGhhbmRsZSA8c2VsZWN0PiBjb3JyZWN0bHk6IGh0dHBzOi8vZ2l0aHViLmNvbS9hc3BuZXQvQmxhem9yL2lzc3Vlcy8xNTdcclxuICAgICAgICAgIChlbGVtZW50IGFzIGFueSkudmFsdWUgPSB2YWx1ZTtcclxuICAgICAgICB9XHJcbiAgICAgICAgcmV0dXJuIHRydWU7XHJcbiAgICAgIGRlZmF1bHQ6XHJcbiAgICAgICAgcmV0dXJuIGZhbHNlO1xyXG4gICAgfVxyXG4gIH1cclxuXHJcbiAgaW5zZXJ0RnJhbWVSYW5nZShjb21wb25lbnRJZDogbnVtYmVyLCBwYXJlbnQ6IEVsZW1lbnQsIGNoaWxkSW5kZXg6IG51bWJlciwgZnJhbWVzOiBTeXN0ZW1fQXJyYXk8UmVuZGVyVHJlZUZyYW1lUG9pbnRlcj4sIHN0YXJ0SW5kZXg6IG51bWJlciwgZW5kSW5kZXhFeGNsOiBudW1iZXIpOiBudW1iZXIge1xyXG4gICAgY29uc3Qgb3JpZ0NoaWxkSW5kZXggPSBjaGlsZEluZGV4O1xyXG4gICAgZm9yIChsZXQgaW5kZXggPSBzdGFydEluZGV4OyBpbmRleCA8IGVuZEluZGV4RXhjbDsgaW5kZXgrKykge1xyXG4gICAgICBjb25zdCBmcmFtZSA9IGdldFRyZWVGcmFtZVB0cihmcmFtZXMsIGluZGV4KTtcclxuICAgICAgY29uc3QgbnVtQ2hpbGRyZW5JbnNlcnRlZCA9IHRoaXMuaW5zZXJ0RnJhbWUoY29tcG9uZW50SWQsIHBhcmVudCwgY2hpbGRJbmRleCwgZnJhbWVzLCBmcmFtZSwgaW5kZXgpO1xyXG4gICAgICBjaGlsZEluZGV4ICs9IG51bUNoaWxkcmVuSW5zZXJ0ZWQ7XHJcblxyXG4gICAgICAvLyBTa2lwIG92ZXIgYW55IGRlc2NlbmRhbnRzLCBzaW5jZSB0aGV5IGFyZSBhbHJlYWR5IGRlYWx0IHdpdGggcmVjdXJzaXZlbHlcclxuICAgICAgY29uc3Qgc3VidHJlZUxlbmd0aCA9IHJlbmRlclRyZWVGcmFtZS5zdWJ0cmVlTGVuZ3RoKGZyYW1lKTtcclxuICAgICAgaWYgKHN1YnRyZWVMZW5ndGggPiAxKSB7XHJcbiAgICAgICAgaW5kZXggKz0gc3VidHJlZUxlbmd0aCAtIDE7XHJcbiAgICAgIH1cclxuICAgIH1cclxuXHJcbiAgICByZXR1cm4gKGNoaWxkSW5kZXggLSBvcmlnQ2hpbGRJbmRleCk7IC8vIFRvdGFsIG51bWJlciBvZiBjaGlsZHJlbiBpbnNlcnRlZFxyXG4gIH1cclxufVxyXG5cclxuZnVuY3Rpb24gaXNDaGVja2JveChlbGVtZW50OiBFbGVtZW50KSB7XHJcbiAgcmV0dXJuIGVsZW1lbnQudGFnTmFtZSA9PT0gJ0lOUFVUJyAmJiBlbGVtZW50LmdldEF0dHJpYnV0ZSgndHlwZScpID09PSAnY2hlY2tib3gnO1xyXG59XHJcblxyXG5mdW5jdGlvbiBpbnNlcnROb2RlSW50b0RPTShub2RlOiBOb2RlLCBwYXJlbnQ6IEVsZW1lbnQsIGNoaWxkSW5kZXg6IG51bWJlcikge1xyXG4gIGlmIChjaGlsZEluZGV4ID49IHBhcmVudC5jaGlsZE5vZGVzLmxlbmd0aCkge1xyXG4gICAgcGFyZW50LmFwcGVuZENoaWxkKG5vZGUpO1xyXG4gIH0gZWxzZSB7XHJcbiAgICBwYXJlbnQuaW5zZXJ0QmVmb3JlKG5vZGUsIHBhcmVudC5jaGlsZE5vZGVzW2NoaWxkSW5kZXhdKTtcclxuICB9XHJcbn1cclxuXHJcbmZ1bmN0aW9uIHJlbW92ZU5vZGVGcm9tRE9NKHBhcmVudDogRWxlbWVudCwgY2hpbGRJbmRleDogbnVtYmVyKSB7XHJcbiAgcGFyZW50LnJlbW92ZUNoaWxkKHBhcmVudC5jaGlsZE5vZGVzW2NoaWxkSW5kZXhdKTtcclxufVxyXG5cclxuZnVuY3Rpb24gcmVtb3ZlQXR0cmlidXRlRnJvbURPTShwYXJlbnQ6IEVsZW1lbnQsIGNoaWxkSW5kZXg6IG51bWJlciwgYXR0cmlidXRlTmFtZTogc3RyaW5nKSB7XHJcbiAgY29uc3QgZWxlbWVudCA9IHBhcmVudC5jaGlsZE5vZGVzW2NoaWxkSW5kZXhdIGFzIEVsZW1lbnQ7XHJcbiAgZWxlbWVudC5yZW1vdmVBdHRyaWJ1dGUoYXR0cmlidXRlTmFtZSk7XHJcbn1cclxuXHJcbmZ1bmN0aW9uIHJhaXNlRXZlbnQoZXZlbnQ6IEV2ZW50LCBicm93c2VyUmVuZGVyZXJJZDogbnVtYmVyLCBjb21wb25lbnRJZDogbnVtYmVyLCBldmVudEhhbmRsZXJJZDogbnVtYmVyLCBldmVudEluZm9UeXBlOiBFdmVudEluZm9UeXBlLCBldmVudEluZm86IGFueSkge1xyXG4gIGV2ZW50LnByZXZlbnREZWZhdWx0KCk7XHJcblxyXG4gIGlmICghcmFpc2VFdmVudE1ldGhvZCkge1xyXG4gICAgcmFpc2VFdmVudE1ldGhvZCA9IHBsYXRmb3JtLmZpbmRNZXRob2QoXHJcbiAgICAgICdNaWNyb3NvZnQuQXNwTmV0Q29yZS5CbGF6b3IuQnJvd3NlcicsICdNaWNyb3NvZnQuQXNwTmV0Q29yZS5CbGF6b3IuQnJvd3Nlci5SZW5kZXJpbmcnLCAnQnJvd3NlclJlbmRlcmVyRXZlbnREaXNwYXRjaGVyJywgJ0Rpc3BhdGNoRXZlbnQnXHJcbiAgICApO1xyXG4gIH1cclxuXHJcbiAgY29uc3QgZXZlbnREZXNjcmlwdG9yID0ge1xyXG4gICAgQnJvd3NlclJlbmRlcmVySWQ6IGJyb3dzZXJSZW5kZXJlcklkLFxyXG4gICAgQ29tcG9uZW50SWQ6IGNvbXBvbmVudElkLFxyXG4gICAgRXZlbnRIYW5kbGVySWQ6IGV2ZW50SGFuZGxlcklkLFxyXG4gICAgRXZlbnRBcmdzVHlwZTogZXZlbnRJbmZvVHlwZVxyXG4gIH07XHJcblxyXG4gIHBsYXRmb3JtLmNhbGxNZXRob2QocmFpc2VFdmVudE1ldGhvZCwgbnVsbCwgW1xyXG4gICAgcGxhdGZvcm0udG9Eb3ROZXRTdHJpbmcoSlNPTi5zdHJpbmdpZnkoZXZlbnREZXNjcmlwdG9yKSksXHJcbiAgICBwbGF0Zm9ybS50b0RvdE5ldFN0cmluZyhKU09OLnN0cmluZ2lmeShldmVudEluZm8pKVxyXG4gIF0pO1xyXG59XHJcblxyXG50eXBlIEV2ZW50SW5mb1R5cGUgPSAnbW91c2UnIHwgJ2tleWJvYXJkJyB8ICdjaGFuZ2UnO1xyXG5cblxuXG4vLyBXRUJQQUNLIEZPT1RFUiAvL1xuLy8gLi9zcmMvUmVuZGVyaW5nL0Jyb3dzZXJSZW5kZXJlci50cyIsImltcG9ydCB7IFN5c3RlbV9BcnJheSwgUG9pbnRlciB9IGZyb20gJy4uL1BsYXRmb3JtL1BsYXRmb3JtJztcclxuaW1wb3J0IHsgcGxhdGZvcm0gfSBmcm9tICcuLi9FbnZpcm9ubWVudCc7XHJcbmNvbnN0IHJlbmRlclRyZWVFZGl0U3RydWN0TGVuZ3RoID0gMTY7XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gZ2V0UmVuZGVyVHJlZUVkaXRQdHIocmVuZGVyVHJlZUVkaXRzOiBTeXN0ZW1fQXJyYXk8UmVuZGVyVHJlZUVkaXRQb2ludGVyPiwgaW5kZXg6IG51bWJlcikge1xyXG4gIHJldHVybiBwbGF0Zm9ybS5nZXRBcnJheUVudHJ5UHRyKHJlbmRlclRyZWVFZGl0cywgaW5kZXgsIHJlbmRlclRyZWVFZGl0U3RydWN0TGVuZ3RoKTtcclxufVxyXG5cclxuZXhwb3J0IGNvbnN0IHJlbmRlclRyZWVFZGl0ID0ge1xyXG4gIC8vIFRoZSBwcm9wZXJ0aWVzIGFuZCBtZW1vcnkgbGF5b3V0IG11c3QgYmUga2VwdCBpbiBzeW5jIHdpdGggdGhlIC5ORVQgZXF1aXZhbGVudCBpbiBSZW5kZXJUcmVlRWRpdC5jc1xyXG4gIHR5cGU6IChlZGl0OiBSZW5kZXJUcmVlRWRpdFBvaW50ZXIpID0+IHBsYXRmb3JtLnJlYWRJbnQzMkZpZWxkKGVkaXQsIDApIGFzIEVkaXRUeXBlLFxyXG4gIHNpYmxpbmdJbmRleDogKGVkaXQ6IFJlbmRlclRyZWVFZGl0UG9pbnRlcikgPT4gcGxhdGZvcm0ucmVhZEludDMyRmllbGQoZWRpdCwgNCksXHJcbiAgbmV3VHJlZUluZGV4OiAoZWRpdDogUmVuZGVyVHJlZUVkaXRQb2ludGVyKSA9PiBwbGF0Zm9ybS5yZWFkSW50MzJGaWVsZChlZGl0LCA4KSxcclxuICByZW1vdmVkQXR0cmlidXRlTmFtZTogKGVkaXQ6IFJlbmRlclRyZWVFZGl0UG9pbnRlcikgPT4gcGxhdGZvcm0ucmVhZFN0cmluZ0ZpZWxkKGVkaXQsIDEyKSxcclxufTtcclxuXHJcbmV4cG9ydCBlbnVtIEVkaXRUeXBlIHtcclxuICBwcmVwZW5kRnJhbWUgPSAxLFxyXG4gIHJlbW92ZUZyYW1lID0gMixcclxuICBzZXRBdHRyaWJ1dGUgPSAzLFxyXG4gIHJlbW92ZUF0dHJpYnV0ZSA9IDQsXHJcbiAgdXBkYXRlVGV4dCA9IDUsXHJcbiAgc3RlcEluID0gNixcclxuICBzdGVwT3V0ID0gNyxcclxufVxyXG5cclxuLy8gTm9taW5hbCB0eXBlIHRvIGVuc3VyZSBvbmx5IHZhbGlkIHBvaW50ZXJzIGFyZSBwYXNzZWQgdG8gdGhlIHJlbmRlclRyZWVFZGl0IGZ1bmN0aW9ucy5cclxuLy8gQXQgcnVudGltZSB0aGUgdmFsdWVzIGFyZSBqdXN0IG51bWJlcnMuXHJcbmV4cG9ydCBpbnRlcmZhY2UgUmVuZGVyVHJlZUVkaXRQb2ludGVyIGV4dGVuZHMgUG9pbnRlciB7IFJlbmRlclRyZWVFZGl0UG9pbnRlcl9fRE9fTk9UX0lNUExFTUVOVDogYW55IH1cclxuXG5cblxuLy8gV0VCUEFDSyBGT09URVIgLy9cbi8vIC4vc3JjL1JlbmRlcmluZy9SZW5kZXJUcmVlRWRpdC50cyIsImltcG9ydCB7IFN5c3RlbV9TdHJpbmcsIFN5c3RlbV9BcnJheSwgUG9pbnRlciB9IGZyb20gJy4uL1BsYXRmb3JtL1BsYXRmb3JtJztcclxuaW1wb3J0IHsgcGxhdGZvcm0gfSBmcm9tICcuLi9FbnZpcm9ubWVudCc7XHJcbmNvbnN0IHJlbmRlclRyZWVGcmFtZVN0cnVjdExlbmd0aCA9IDI4O1xyXG5cclxuLy8gVG8gbWluaW1pc2UgR0MgcHJlc3N1cmUsIGluc3RlYWQgb2YgaW5zdGFudGlhdGluZyBhIEpTIG9iamVjdCB0byByZXByZXNlbnQgZWFjaCB0cmVlIGZyYW1lLFxyXG4vLyB3ZSB3b3JrIGluIHRlcm1zIG9mIHBvaW50ZXJzIHRvIHRoZSBzdHJ1Y3RzIG9uIHRoZSAuTkVUIGhlYXAsIGFuZCB1c2Ugc3RhdGljIGZ1bmN0aW9ucyB0aGF0XHJcbi8vIGtub3cgaG93IHRvIHJlYWQgcHJvcGVydHkgdmFsdWVzIGZyb20gdGhvc2Ugc3RydWN0cy5cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBnZXRUcmVlRnJhbWVQdHIocmVuZGVyVHJlZUVudHJpZXM6IFN5c3RlbV9BcnJheTxSZW5kZXJUcmVlRnJhbWVQb2ludGVyPiwgaW5kZXg6IG51bWJlcikge1xyXG4gIHJldHVybiBwbGF0Zm9ybS5nZXRBcnJheUVudHJ5UHRyKHJlbmRlclRyZWVFbnRyaWVzLCBpbmRleCwgcmVuZGVyVHJlZUZyYW1lU3RydWN0TGVuZ3RoKTtcclxufVxyXG5cclxuZXhwb3J0IGNvbnN0IHJlbmRlclRyZWVGcmFtZSA9IHtcclxuICAvLyBUaGUgcHJvcGVydGllcyBhbmQgbWVtb3J5IGxheW91dCBtdXN0IGJlIGtlcHQgaW4gc3luYyB3aXRoIHRoZSAuTkVUIGVxdWl2YWxlbnQgaW4gUmVuZGVyVHJlZUZyYW1lLmNzXHJcbiAgZnJhbWVUeXBlOiAoZnJhbWU6IFJlbmRlclRyZWVGcmFtZVBvaW50ZXIpID0+IHBsYXRmb3JtLnJlYWRJbnQzMkZpZWxkKGZyYW1lLCA0KSBhcyBGcmFtZVR5cGUsXHJcbiAgc3VidHJlZUxlbmd0aDogKGZyYW1lOiBSZW5kZXJUcmVlRnJhbWVQb2ludGVyKSA9PiBwbGF0Zm9ybS5yZWFkSW50MzJGaWVsZChmcmFtZSwgOCkgYXMgRnJhbWVUeXBlLFxyXG4gIGNvbXBvbmVudElkOiAoZnJhbWU6IFJlbmRlclRyZWVGcmFtZVBvaW50ZXIpID0+IHBsYXRmb3JtLnJlYWRJbnQzMkZpZWxkKGZyYW1lLCAxMiksXHJcbiAgZWxlbWVudE5hbWU6IChmcmFtZTogUmVuZGVyVHJlZUZyYW1lUG9pbnRlcikgPT4gcGxhdGZvcm0ucmVhZFN0cmluZ0ZpZWxkKGZyYW1lLCAxNiksXHJcbiAgdGV4dENvbnRlbnQ6IChmcmFtZTogUmVuZGVyVHJlZUZyYW1lUG9pbnRlcikgPT4gcGxhdGZvcm0ucmVhZFN0cmluZ0ZpZWxkKGZyYW1lLCAxNiksXHJcbiAgYXR0cmlidXRlTmFtZTogKGZyYW1lOiBSZW5kZXJUcmVlRnJhbWVQb2ludGVyKSA9PiBwbGF0Zm9ybS5yZWFkU3RyaW5nRmllbGQoZnJhbWUsIDE2KSxcclxuICBhdHRyaWJ1dGVWYWx1ZTogKGZyYW1lOiBSZW5kZXJUcmVlRnJhbWVQb2ludGVyKSA9PiBwbGF0Zm9ybS5yZWFkU3RyaW5nRmllbGQoZnJhbWUsIDI0KSxcclxuICBhdHRyaWJ1dGVFdmVudEhhbmRsZXJJZDogKGZyYW1lOiBSZW5kZXJUcmVlRnJhbWVQb2ludGVyKSA9PiBwbGF0Zm9ybS5yZWFkSW50MzJGaWVsZChmcmFtZSwgOCksXHJcbn07XHJcblxyXG5leHBvcnQgZW51bSBGcmFtZVR5cGUge1xyXG4gIC8vIFRoZSB2YWx1ZXMgbXVzdCBiZSBrZXB0IGluIHN5bmMgd2l0aCB0aGUgLk5FVCBlcXVpdmFsZW50IGluIFJlbmRlclRyZWVGcmFtZVR5cGUuY3NcclxuICBlbGVtZW50ID0gMSxcclxuICB0ZXh0ID0gMixcclxuICBhdHRyaWJ1dGUgPSAzLFxyXG4gIGNvbXBvbmVudCA9IDQsXHJcbiAgcmVnaW9uID0gNSxcclxufVxyXG5cclxuLy8gTm9taW5hbCB0eXBlIHRvIGVuc3VyZSBvbmx5IHZhbGlkIHBvaW50ZXJzIGFyZSBwYXNzZWQgdG8gdGhlIHJlbmRlclRyZWVGcmFtZSBmdW5jdGlvbnMuXHJcbi8vIEF0IHJ1bnRpbWUgdGhlIHZhbHVlcyBhcmUganVzdCBudW1iZXJzLlxyXG5leHBvcnQgaW50ZXJmYWNlIFJlbmRlclRyZWVGcmFtZVBvaW50ZXIgZXh0ZW5kcyBQb2ludGVyIHsgUmVuZGVyVHJlZUZyYW1lUG9pbnRlcl9fRE9fTk9UX0lNUExFTUVOVDogYW55IH1cclxuXG5cblxuLy8gV0VCUEFDSyBGT09URVIgLy9cbi8vIC4vc3JjL1JlbmRlcmluZy9SZW5kZXJUcmVlRnJhbWUudHMiLCJpbXBvcnQgeyByZWdpc3RlckZ1bmN0aW9uIH0gZnJvbSAnLi4vSW50ZXJvcC9SZWdpc3RlcmVkRnVuY3Rpb24nO1xyXG5pbXBvcnQgeyBwbGF0Zm9ybSB9IGZyb20gJy4uL0Vudmlyb25tZW50JztcclxuaW1wb3J0IHsgTWV0aG9kSGFuZGxlLCBTeXN0ZW1fU3RyaW5nIH0gZnJvbSAnLi4vUGxhdGZvcm0vUGxhdGZvcm0nO1xyXG5jb25zdCBodHRwQ2xpZW50QXNzZW1ibHkgPSAnTWljcm9zb2Z0LkFzcE5ldENvcmUuQmxhem9yLkJyb3dzZXInO1xyXG5jb25zdCBodHRwQ2xpZW50TmFtZXNwYWNlID0gYCR7aHR0cENsaWVudEFzc2VtYmx5fS5IdHRwYDtcclxuY29uc3QgaHR0cENsaWVudFR5cGVOYW1lID0gJ0Jyb3dzZXJIdHRwTWVzc2FnZUhhbmRsZXInO1xyXG5jb25zdCBodHRwQ2xpZW50RnVsbFR5cGVOYW1lID0gYCR7aHR0cENsaWVudE5hbWVzcGFjZX0uJHtodHRwQ2xpZW50VHlwZU5hbWV9YDtcclxubGV0IHJlY2VpdmVSZXNwb25zZU1ldGhvZDogTWV0aG9kSGFuZGxlO1xyXG5cclxucmVnaXN0ZXJGdW5jdGlvbihgJHtodHRwQ2xpZW50RnVsbFR5cGVOYW1lfS5TZW5kYCwgKGlkOiBudW1iZXIsIG1ldGhvZDogc3RyaW5nLCByZXF1ZXN0VXJpOiBzdHJpbmcsIGJvZHk6IHN0cmluZyB8IG51bGwsIGhlYWRlcnNKc29uOiBzdHJpbmcgfCBudWxsKSA9PiB7XHJcbiAgc2VuZEFzeW5jKGlkLCBtZXRob2QsIHJlcXVlc3RVcmksIGJvZHksIGhlYWRlcnNKc29uKTtcclxufSk7XHJcblxyXG5hc3luYyBmdW5jdGlvbiBzZW5kQXN5bmMoaWQ6IG51bWJlciwgbWV0aG9kOiBzdHJpbmcsIHJlcXVlc3RVcmk6IHN0cmluZywgYm9keTogc3RyaW5nIHwgbnVsbCwgaGVhZGVyc0pzb246IHN0cmluZyB8IG51bGwpIHtcclxuICBsZXQgcmVzcG9uc2U6IFJlc3BvbnNlO1xyXG4gIGxldCByZXNwb25zZVRleHQ6IHN0cmluZztcclxuICB0cnkge1xyXG4gICAgcmVzcG9uc2UgPSBhd2FpdCBmZXRjaChyZXF1ZXN0VXJpLCB7XHJcbiAgICAgIG1ldGhvZDogbWV0aG9kLFxyXG4gICAgICBib2R5OiBib2R5IHx8IHVuZGVmaW5lZCxcclxuICAgICAgaGVhZGVyczogaGVhZGVyc0pzb24gPyAoSlNPTi5wYXJzZShoZWFkZXJzSnNvbikgYXMgc3RyaW5nW11bXSkgOiB1bmRlZmluZWRcclxuICAgIH0pO1xyXG4gICAgcmVzcG9uc2VUZXh0ID0gYXdhaXQgcmVzcG9uc2UudGV4dCgpO1xyXG4gIH0gY2F0Y2ggKGV4KSB7XHJcbiAgICBkaXNwYXRjaEVycm9yUmVzcG9uc2UoaWQsIGV4LnRvU3RyaW5nKCkpO1xyXG4gICAgcmV0dXJuO1xyXG4gIH1cclxuXHJcbiAgZGlzcGF0Y2hTdWNjZXNzUmVzcG9uc2UoaWQsIHJlc3BvbnNlLCByZXNwb25zZVRleHQpO1xyXG59XHJcblxyXG5mdW5jdGlvbiBkaXNwYXRjaFN1Y2Nlc3NSZXNwb25zZShpZDogbnVtYmVyLCByZXNwb25zZTogUmVzcG9uc2UsIHJlc3BvbnNlVGV4dDogc3RyaW5nKSB7XHJcbiAgY29uc3QgcmVzcG9uc2VEZXNjcmlwdG9yOiBSZXNwb25zZURlc2NyaXB0b3IgPSB7XHJcbiAgICBTdGF0dXNDb2RlOiByZXNwb25zZS5zdGF0dXMsXHJcbiAgICBIZWFkZXJzOiBbXVxyXG4gIH07XHJcbiAgcmVzcG9uc2UuaGVhZGVycy5mb3JFYWNoKCh2YWx1ZSwgbmFtZSkgPT4ge1xyXG4gICAgcmVzcG9uc2VEZXNjcmlwdG9yLkhlYWRlcnMucHVzaChbbmFtZSwgdmFsdWVdKTtcclxuICB9KTtcclxuXHJcbiAgZGlzcGF0Y2hSZXNwb25zZShcclxuICAgIGlkLFxyXG4gICAgcGxhdGZvcm0udG9Eb3ROZXRTdHJpbmcoSlNPTi5zdHJpbmdpZnkocmVzcG9uc2VEZXNjcmlwdG9yKSksXHJcbiAgICBwbGF0Zm9ybS50b0RvdE5ldFN0cmluZyhyZXNwb25zZVRleHQpLCAvLyBUT0RPOiBDb25zaWRlciBob3cgdG8gaGFuZGxlIG5vbi1zdHJpbmcgcmVzcG9uc2VzXHJcbiAgICAvKiBlcnJvck1lc3NhZ2UgKi8gbnVsbFxyXG4gICk7XHJcbn1cclxuXHJcbmZ1bmN0aW9uIGRpc3BhdGNoRXJyb3JSZXNwb25zZShpZDogbnVtYmVyLCBlcnJvck1lc3NhZ2U6IHN0cmluZykge1xyXG4gIGRpc3BhdGNoUmVzcG9uc2UoXHJcbiAgICBpZCxcclxuICAgIC8qIHJlc3BvbnNlRGVzY3JpcHRvciAqLyBudWxsLFxyXG4gICAgLyogcmVzcG9uc2VUZXh0ICovIG51bGwsXHJcbiAgICBwbGF0Zm9ybS50b0RvdE5ldFN0cmluZyhlcnJvck1lc3NhZ2UpXHJcbiAgKTtcclxufVxyXG5cclxuZnVuY3Rpb24gZGlzcGF0Y2hSZXNwb25zZShpZDogbnVtYmVyLCByZXNwb25zZURlc2NyaXB0b3I6IFN5c3RlbV9TdHJpbmcgfCBudWxsLCByZXNwb25zZVRleHQ6IFN5c3RlbV9TdHJpbmcgfCBudWxsLCBlcnJvck1lc3NhZ2U6IFN5c3RlbV9TdHJpbmcgfCBudWxsKSB7XHJcbiAgaWYgKCFyZWNlaXZlUmVzcG9uc2VNZXRob2QpIHtcclxuICAgIHJlY2VpdmVSZXNwb25zZU1ldGhvZCA9IHBsYXRmb3JtLmZpbmRNZXRob2QoXHJcbiAgICAgIGh0dHBDbGllbnRBc3NlbWJseSxcclxuICAgICAgaHR0cENsaWVudE5hbWVzcGFjZSxcclxuICAgICAgaHR0cENsaWVudFR5cGVOYW1lLFxyXG4gICAgICAnUmVjZWl2ZVJlc3BvbnNlJ1xyXG4gICAgKTtcclxuICB9XHJcblxyXG4gIHBsYXRmb3JtLmNhbGxNZXRob2QocmVjZWl2ZVJlc3BvbnNlTWV0aG9kLCBudWxsLCBbXHJcbiAgICBwbGF0Zm9ybS50b0RvdE5ldFN0cmluZyhpZC50b1N0cmluZygpKSxcclxuICAgIHJlc3BvbnNlRGVzY3JpcHRvcixcclxuICAgIHJlc3BvbnNlVGV4dCxcclxuICAgIGVycm9yTWVzc2FnZSxcclxuICBdKTtcclxufVxyXG5cclxuLy8gS2VlcCB0aGlzIGluIHN5bmMgd2l0aCB0aGUgLk5FVCBlcXVpdmFsZW50IGluIEh0dHBDbGllbnQuY3NcclxuaW50ZXJmYWNlIFJlc3BvbnNlRGVzY3JpcHRvciB7XHJcbiAgLy8gV2UgZG9uJ3QgaGF2ZSBCb2R5VGV4dCBpbiBoZXJlIGJlY2F1c2UgaWYgd2UgZGlkLCB0aGVuIGluIHRoZSBKU09OLXJlc3BvbnNlIGNhc2UgKHdoaWNoXHJcbiAgLy8gaXMgdGhlIG1vc3QgY29tbW9uIGNhc2UpLCB3ZSdkIGJlIGRvdWJsZS1lbmNvZGluZyBpdCwgc2luY2UgdGhlIGVudGlyZSBSZXNwb25zZURlc2NyaXB0b3JcclxuICAvLyBhbHNvIGdldHMgSlNPTiBlbmNvZGVkLiBJdCB3b3VsZCB3b3JrIGJ1dCBpcyB0d2ljZSB0aGUgYW1vdW50IG9mIHN0cmluZyBwcm9jZXNzaW5nLlxyXG4gIFN0YXR1c0NvZGU6IG51bWJlcjtcclxuICBIZWFkZXJzOiBzdHJpbmdbXVtdO1xyXG59XHJcblxuXG5cbi8vIFdFQlBBQ0sgRk9PVEVSIC8vXG4vLyAuL3NyYy9TZXJ2aWNlcy9IdHRwLnRzIiwiaW1wb3J0IHsgcmVnaXN0ZXJGdW5jdGlvbiB9IGZyb20gJy4uL0ludGVyb3AvUmVnaXN0ZXJlZEZ1bmN0aW9uJztcclxuaW1wb3J0IHsgcGxhdGZvcm0gfSBmcm9tICcuLi9FbnZpcm9ubWVudCc7XHJcbmltcG9ydCB7IE1ldGhvZEhhbmRsZSB9IGZyb20gJy4uL1BsYXRmb3JtL1BsYXRmb3JtJztcclxuY29uc3QgcmVnaXN0ZXJlZEZ1bmN0aW9uUHJlZml4ID0gJ01pY3Jvc29mdC5Bc3BOZXRDb3JlLkJsYXpvci5Ccm93c2VyLlNlcnZpY2VzLkJyb3dzZXJVcmlIZWxwZXInO1xyXG5sZXQgbm90aWZ5TG9jYXRpb25DaGFuZ2VkTWV0aG9kOiBNZXRob2RIYW5kbGU7XHJcbmxldCBoYXNSZWdpc3RlcmVkRXZlbnRMaXN0ZW5lcnMgPSBmYWxzZTtcclxuXHJcbnJlZ2lzdGVyRnVuY3Rpb24oYCR7cmVnaXN0ZXJlZEZ1bmN0aW9uUHJlZml4fS5nZXRMb2NhdGlvbkhyZWZgLFxyXG4gICgpID0+IHBsYXRmb3JtLnRvRG90TmV0U3RyaW5nKGxvY2F0aW9uLmhyZWYpKTtcclxuXHJcbnJlZ2lzdGVyRnVuY3Rpb24oYCR7cmVnaXN0ZXJlZEZ1bmN0aW9uUHJlZml4fS5nZXRCYXNlVVJJYCxcclxuICAoKSA9PiBkb2N1bWVudC5iYXNlVVJJID8gcGxhdGZvcm0udG9Eb3ROZXRTdHJpbmcoZG9jdW1lbnQuYmFzZVVSSSkgOiBudWxsKTtcclxuXHJcbnJlZ2lzdGVyRnVuY3Rpb24oYCR7cmVnaXN0ZXJlZEZ1bmN0aW9uUHJlZml4fS5lbmFibGVOYXZpZ2F0aW9uSW50ZWNlcHRpb25gLCAoKSA9PiB7XHJcbiAgaWYgKGhhc1JlZ2lzdGVyZWRFdmVudExpc3RlbmVycykge1xyXG4gICAgcmV0dXJuO1xyXG4gIH1cclxuICBoYXNSZWdpc3RlcmVkRXZlbnRMaXN0ZW5lcnMgPSB0cnVlO1xyXG5cclxuICBkb2N1bWVudC5hZGRFdmVudExpc3RlbmVyKCdjbGljaycsIGV2ZW50ID0+IHtcclxuICAgIC8vIEludGVyY2VwdCBjbGlja3Mgb24gYWxsIDxhPiBlbGVtZW50cyB3aGVyZSB0aGUgaHJlZiBpcyB3aXRoaW4gdGhlIDxiYXNlIGhyZWY+IFVSSSBzcGFjZVxyXG4gICAgY29uc3QgYW5jaG9yVGFyZ2V0ID0gZmluZENsb3Nlc3RBbmNlc3RvcihldmVudC50YXJnZXQgYXMgRWxlbWVudCB8IG51bGwsICdBJyk7XHJcbiAgICBpZiAoYW5jaG9yVGFyZ2V0KSB7XHJcbiAgICAgIGNvbnN0IGhyZWYgPSBhbmNob3JUYXJnZXQuZ2V0QXR0cmlidXRlKCdocmVmJyk7XHJcbiAgICAgIGlmIChpc1dpdGhpbkJhc2VVcmlTcGFjZSh0b0Fic29sdXRlVXJpKGhyZWYpKSkge1xyXG4gICAgICAgIGV2ZW50LnByZXZlbnREZWZhdWx0KCk7XHJcbiAgICAgICAgaGlzdG9yeS5wdXNoU3RhdGUobnVsbCwgLyogaWdub3JlZCB0aXRsZSAqLyAnJywgaHJlZik7XHJcbiAgICAgICAgaGFuZGxlSW50ZXJuYWxOYXZpZ2F0aW9uKCk7XHJcbiAgICAgIH1cclxuICAgIH1cclxuICB9KTtcclxuXHJcbiAgd2luZG93LmFkZEV2ZW50TGlzdGVuZXIoJ3BvcHN0YXRlJywgaGFuZGxlSW50ZXJuYWxOYXZpZ2F0aW9uKTtcclxufSk7XHJcblxyXG5mdW5jdGlvbiBoYW5kbGVJbnRlcm5hbE5hdmlnYXRpb24oKSB7XHJcbiAgaWYgKCFub3RpZnlMb2NhdGlvbkNoYW5nZWRNZXRob2QpIHtcclxuICAgIG5vdGlmeUxvY2F0aW9uQ2hhbmdlZE1ldGhvZCA9IHBsYXRmb3JtLmZpbmRNZXRob2QoXHJcbiAgICAgICdNaWNyb3NvZnQuQXNwTmV0Q29yZS5CbGF6b3IuQnJvd3NlcicsXHJcbiAgICAgICdNaWNyb3NvZnQuQXNwTmV0Q29yZS5CbGF6b3IuQnJvd3Nlci5TZXJ2aWNlcycsXHJcbiAgICAgICdCcm93c2VyVXJpSGVscGVyJyxcclxuICAgICAgJ05vdGlmeUxvY2F0aW9uQ2hhbmdlZCdcclxuICAgICk7XHJcbiAgfVxyXG5cclxuICBwbGF0Zm9ybS5jYWxsTWV0aG9kKG5vdGlmeUxvY2F0aW9uQ2hhbmdlZE1ldGhvZCwgbnVsbCwgW1xyXG4gICAgcGxhdGZvcm0udG9Eb3ROZXRTdHJpbmcobG9jYXRpb24uaHJlZilcclxuICBdKTtcclxufVxyXG5cclxubGV0IHRlc3RBbmNob3I6IEhUTUxBbmNob3JFbGVtZW50O1xyXG5mdW5jdGlvbiB0b0Fic29sdXRlVXJpKHJlbGF0aXZlVXJpOiBzdHJpbmcpIHtcclxuICB0ZXN0QW5jaG9yID0gdGVzdEFuY2hvciB8fCBkb2N1bWVudC5jcmVhdGVFbGVtZW50KCdhJyk7XHJcbiAgdGVzdEFuY2hvci5ocmVmID0gcmVsYXRpdmVVcmk7XHJcbiAgcmV0dXJuIHRlc3RBbmNob3IuaHJlZjtcclxufVxyXG5cclxuZnVuY3Rpb24gZmluZENsb3Nlc3RBbmNlc3RvcihlbGVtZW50OiBFbGVtZW50IHwgbnVsbCwgdGFnTmFtZTogc3RyaW5nKSB7XHJcbiAgcmV0dXJuICFlbGVtZW50XHJcbiAgICA/IG51bGxcclxuICAgIDogZWxlbWVudC50YWdOYW1lID09PSB0YWdOYW1lXHJcbiAgICAgID8gZWxlbWVudFxyXG4gICAgICA6IGZpbmRDbG9zZXN0QW5jZXN0b3IoZWxlbWVudC5wYXJlbnRFbGVtZW50LCB0YWdOYW1lKVxyXG59XHJcblxyXG5mdW5jdGlvbiBpc1dpdGhpbkJhc2VVcmlTcGFjZShocmVmOiBzdHJpbmcpIHtcclxuICBjb25zdCBiYXNlVXJpUHJlZml4V2l0aFRyYWlsaW5nU2xhc2ggPSB0b0Jhc2VVcmlQcmVmaXhXaXRoVHJhaWxpbmdTbGFzaChkb2N1bWVudC5iYXNlVVJJISk7IC8vIFRPRE86IE1pZ2h0IGJhc2VVUkkgcmVhbGx5IGJlIG51bGw/XHJcbiAgcmV0dXJuIGhyZWYuc3RhcnRzV2l0aChiYXNlVXJpUHJlZml4V2l0aFRyYWlsaW5nU2xhc2gpO1xyXG59XHJcblxyXG5mdW5jdGlvbiB0b0Jhc2VVcmlQcmVmaXhXaXRoVHJhaWxpbmdTbGFzaChiYXNlVXJpOiBzdHJpbmcpIHtcclxuICByZXR1cm4gYmFzZVVyaS5zdWJzdHIoMCwgYmFzZVVyaS5sYXN0SW5kZXhPZignLycpICsgMSk7XHJcbn1cclxuXG5cblxuLy8gV0VCUEFDSyBGT09URVIgLy9cbi8vIC4vc3JjL1NlcnZpY2VzL1VyaUhlbHBlci50cyIsImltcG9ydCB7IHBsYXRmb3JtIH0gZnJvbSAnLi9FbnZpcm9ubWVudCdcclxuaW1wb3J0IHsgcmVnaXN0ZXJGdW5jdGlvbiB9IGZyb20gJy4vSW50ZXJvcC9SZWdpc3RlcmVkRnVuY3Rpb24nO1xyXG5cclxuaWYgKHR5cGVvZiB3aW5kb3cgIT09ICd1bmRlZmluZWQnKSB7XHJcbiAgLy8gV2hlbiB0aGUgbGlicmFyeSBpcyBsb2FkZWQgaW4gYSBicm93c2VyIHZpYSBhIDxzY3JpcHQ+IGVsZW1lbnQsIG1ha2UgdGhlXHJcbiAgLy8gZm9sbG93aW5nIEFQSXMgYXZhaWxhYmxlIGluIGdsb2JhbCBzY29wZSBmb3IgaW52b2NhdGlvbiBmcm9tIEpTXHJcbiAgd2luZG93WydCbGF6b3InXSA9IHtcclxuICAgIHBsYXRmb3JtLFxyXG4gICAgcmVnaXN0ZXJGdW5jdGlvbixcclxuICB9O1xyXG59XHJcblxuXG5cbi8vIFdFQlBBQ0sgRk9PVEVSIC8vXG4vLyAuL3NyYy9HbG9iYWxFeHBvcnRzLnRzIl0sInNvdXJjZVJvb3QiOiIifQ==