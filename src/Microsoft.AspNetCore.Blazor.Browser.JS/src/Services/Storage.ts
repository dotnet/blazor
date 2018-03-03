import { registerFunction } from '../Interop/RegisteredFunction';

const storageAssembly = 'Microsoft.AspNetCore.Blazor.Browser';
const storageNamespace = `${storageAssembly}.Storage`;

const storages: { [key: string]: Storage } = {
    LocalStorage: localStorage,
    SessionStorage: sessionStorage
};

for (var storageTypeName in storages) {
    const storage = storages[storageTypeName];
    const storageFullTypeName = `${storageNamespace}.${storageTypeName}`;

    registerFunction(`${storageFullTypeName}.Clear`, () => {
        clear(storage);
    });

    registerFunction(`${storageFullTypeName}.GetItem`, (key: string) => {
        return getItem(storage, key);
    });

    registerFunction(`${storageFullTypeName}.Key`, (index: number) => {
        return key(storage, index);
    });

    registerFunction(`${storageFullTypeName}.Length`, () => {
        return length(storage);
    });

    registerFunction(`${storageFullTypeName}.RemoveItem`, (key: string) => {
        removeItem(storage, key);
    });

    registerFunction(`${storageFullTypeName}.SetItem`, (key: string, data: any) => {
        setItem(storage, key, data);
    });

    registerFunction(`${storageFullTypeName}.GetItemString`, (key: string) => {
        return getItemString(storage, key);
    });

    registerFunction(`${storageFullTypeName}.SetItemString`, (key: string, data: string) => {
        setItemString(storage, key, data);
    });

    registerFunction(`${storageFullTypeName}.GetItemNumber`, (index: number) => {
        return getItemNumber(storage, index);
    });

    registerFunction(`${storageFullTypeName}.SetItemNumber`, (index: number, data: string) => {
        setItemNumber(storage, index, data);
    });
}

function clear(storage: Storage) {
    storage.clear();
}

function getItem(storage: Storage, key: string) {
    return storage.getItem(key);
}

function key(storage: Storage, index: number) {
    return storage.key(index);
}

function length(storage: Storage) {
    return storage.length;
}

function removeItem(storage: Storage, key: string) {
    storage.removeItem(key);
}

function setItem(storage: Storage, key: string, data: any) {
    storage.setItem(key, data)
}

function getItemString(storage: Storage, key: string) {
    return storage[key];
}

function setItemString(storage: Storage, key: string, data: any) {
    storage[key] = data;
}

function getItemNumber(storage: Storage, index: number) {
    return storage[index]
}

function setItemNumber(storage: Storage, index: number, data: string) {
    storage[index] = data;
}