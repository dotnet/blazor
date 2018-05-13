import { MethodOptions, DotnetMethodArgumentsList } from './MethodOptions';
import { platform } from '../Environment';
import { System_String } from '../Platform/Platform';
import { getRegisteredFunction } from './RegisteredFunction';

export function invokeDotNetMethod<T>(methodOptions: MethodOptions, args: DotnetMethodArgumentsList = {}): (T | null) {
    const method = platform.findMethod(
        "Microsoft.AspNetCore.Blazor.Browser",
        "Microsoft.AspNetCore.Blazor.Browser.Interop",
        "JavaScriptInvoke",
        "InvokeDotnetMethod");

    const serializedOptions = platform.toDotNetString(JSON.stringify(methodOptions));
    const serializedArgs = platform.toDotNetString(JSON.stringify(args));
    const serializedResult = platform.callMethod(method, null, [serializedOptions, serializedArgs]);

    if (serializedResult !== null && serializedResult !== undefined && (serializedResult as any) !== 0) {
        const result = JSON.parse(platform.toJavaScriptString(serializedResult as System_String));
        return result;
    }

    return null;
}

let globalId = 0;

export function invokeDotNetMethodAsync<T>(methodOptions: MethodOptions, args: DotnetMethodArgumentsList = {}): Promise<T | null> {
    const resolveId = (globalId++).toString();
    const rejectId = (globalId++).toString();
    methodOptions.Async = { ResolveId: resolveId, RejectId: rejectId, FunctionName: "invokeJavaScriptCallback" };

    const result = new Promise<T | null>((resolve, reject) => {
        TrackedReference.track(resolveId, resolve);
        TrackedReference.track(rejectId, reject);
    });

    invokeDotNetMethod(methodOptions, args);

    return result;
}

export function invokeJavaScriptCallback(id: string, ...args: any[]): void {
    const callbackRef = TrackedReference.get(id);
    const callback = callbackRef.trackedObject as Function;
    callback.apply(null, args);
}

type RefType = Exclude<any, undefined | null>;

class TrackedReference {
    private static references: Map<string, RefType> = new Map<string, RefType>();

    private constructor(public id: string, public trackedObject: RefType) {
    }

    public static track(id: string, trackedObject: RefType): void {
        const ref = new TrackedReference(id, trackedObject);
        const refs = TrackedReference.references;
        if (refs.has(id)) {
            throw new Error(`An element with id '${id}' is already being tracked.`);
        }

        refs.set(id, ref);
    }

    public static untrack(id: string): void {
        const refs = TrackedReference.references;
        if (!refs.has(id)) {
            throw new Error(`An element with id '${id}' is not being being tracked.`);
        }

        refs.delete(id);
    }

    public static get(id: string): TrackedReference {
        const refs = TrackedReference.references;
        if (!refs.has(id)) {
            throw new Error(`An element with id '${id}' is not being being tracked.`);
        }

        return refs.get(id);
    }
}