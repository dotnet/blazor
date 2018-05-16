import { platform } from './Environment';
import { getAssemblyNameFromUrl } from './Platform/DotNet';
import './Rendering/Renderer';
import './Services/Http';
import './Services/UriHelper';
import './GlobalExports';

async function boot() {
  // Read startup config from the <script> element that's importing this file
  const allScriptElems = document.getElementsByTagName('script');
  const thisScriptElem = (document.currentScript || allScriptElems[allScriptElems.length - 1]) as HTMLScriptElement;
  const onBlazorInitializing = thisScriptElem.getAttribute('on-blazor-initializing');
  const onBlazorInitialized = thisScriptElem.getAttribute('on-blazor-initialized');
  const isLinkerEnabled = thisScriptElem.getAttribute('linker-enabled') === 'true';
  const entryPointDll = getRequiredBootScriptAttribute(thisScriptElem, 'main');
  const entryPointMethod = getRequiredBootScriptAttribute(thisScriptElem, 'entrypoint');
  const entryPointAssemblyName = getAssemblyNameFromUrl(entryPointDll);
  const referenceAssembliesCommaSeparated = thisScriptElem.getAttribute('references') || '';
  const referenceAssemblies = referenceAssembliesCommaSeparated
    .split(',')
    .map(s => s.trim())
    .filter(s => !!s);

  if (!isLinkerEnabled) {
    console.info('Blazor is running in dev mode without IL stripping. To make the bundle size significantly smaller, publish the application or see https://go.microsoft.com/fwlink/?linkid=870414');
  }

  // Determine the URLs of the assemblies we want to load
  const loadAssemblyUrls = [entryPointDll]
    .concat(referenceAssemblies)
    .map(filename => `_framework/_bin/${filename}`);

  try {
    await platform.start(loadAssemblyUrls);
  } catch (ex) {
    throw new Error(`Failed to start platform. Reason: ${ex}`);
  }

  if (onBlazorInitializing) {
    if (!window[onBlazorInitializing]) {
      throw new Error(`Failed to find a function ${onBlazorInitializing} in 'window'`);
    }

    const blazor = window['Blazor'];
    const customInitialization = window[onBlazorInitializing] as Function;
    customInitialization.call(null, blazor);
  }

  // Start up the application
  platform.callEntryPoint(entryPointAssemblyName, entryPointMethod, []);

  if (onBlazorInitialized) {
    if (!window[onBlazorInitialized]) {
      throw new Error(`Failed to find a function ${onBlazorInitialized} in 'window'`);
    }

    const blazor = window['Blazor'];
    const customInitialization = window[onBlazorInitialized] as Function;
    customInitialization.apply(null, blazor);
  }
}

function getRequiredBootScriptAttribute(elem: HTMLScriptElement, attributeName: string): string {
  const result = elem.getAttribute(attributeName);
  if (!result) {
    throw new Error(`Missing "${attributeName}" attribute on Blazor script tag.`);
  }
  return result;
}

boot();
