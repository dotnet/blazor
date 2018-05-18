
function __interop__testing_getTree() {
  return {
    root: {
      id: "90",
      value: 1,
      children: [
        {
          id: "1.1",
          value: 2,
          children: [
            {
              id: "1.1.1",
              value: 3
            },
            {
              id: "1.1.2",
              value: 3
            }]
        },
        {
          id: "1.2",
          value: 2,
          children: [
            {
              id: "1.2.1",
              value: 3
            },
            {
              id: "1.2.2",
              value: 3
            }]
        }]
    }
  }
}

function __interop__testing_getTreeAsync() {
  return Promise.resolve({
    root: {
      id: "1",
      value: 1,
      children: [
        {
          id: "1.1",
          value: 2,
          children: [
            {
              id: "1.1.1",
              value: 3
            },
            {
              id: "1.1.2",
              value: 3
            }]
        },
        {
          id: "1.2",
          value: 2,
          children: [
            {
              id: "1.2.1",
              value: 3
            },
            {
              id: "1.2.2",
              value: 3
            }]
        }]
    }
  });
}

Blazor.registerFunction("Interop.Testing.GetTree", __interop__testing_getTree);
Blazor.registerFunction("Interop.Testing.GetTreeAsync", __interop__testing_getTreeAsync);