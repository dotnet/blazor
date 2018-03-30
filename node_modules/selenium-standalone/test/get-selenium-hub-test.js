var path = require('path');
var assert = require("assert");
var statusUrl = require('../lib/get-selenium-status-url');

var nodeStatusAPIPath = '/wd/hub/status';
var hubStatusAPIPath = '/grid/api/hub';
describe('getRunningProcessType', function () {
  var tests = [
    // Started as a standalone Selenium Server
    {args: [], expected: statusUrl.PROCESS_TYPES.STANDALONE},
    {args: ['-port', '5555'], expected: statusUrl.PROCESS_TYPES.STANDALONE},
    {args: ['-hub', 'https://foo/wd/register'], expected: statusUrl.PROCESS_TYPES.STANDALONE}, // `-hub` arg is ignored

    // Started as a Selenium Grid hub
    {args: ['-role', 'hub'], expected: statusUrl.PROCESS_TYPES.GRID_HUB},

    // Started as a Selenium Grid node
    {args: ['-role', 'node'], expected: statusUrl.PROCESS_TYPES.GRID_NODE},
    {args: ['-role', 'node', '-hub', 'https://foo/wd/register'], expected: statusUrl.PROCESS_TYPES.GRID_NODE},
  ];

  tests.forEach(function(test) {
    it('getRunningProcessType with seleniumArgs: ' + test.args.join(' '), function() {
      var actual = statusUrl.getRunningProcessType(test.args);
      assert.equal(actual, test.expected);
    });
  });
});

describe('getSeleniumStatusUrl', function () {
  var data = [
              // Started as a standalone Selenium Server
              {args: [], expectedUrl: 'localhost:4444' + nodeStatusAPIPath},
              {args: ['-port', '5678'], expectedUrl: 'localhost:5678' + nodeStatusAPIPath},
              {args: ['-hub', 'https://foo/wd/register'], expectedUrl: 'localhost:4444' + nodeStatusAPIPath},
              {args: ['-hub', 'https://foo:6666/wd/register', '-port', '7777'], expectedUrl: 'localhost:7777' + nodeStatusAPIPath},

              // Started as a Selenium Grid hub
              {args: ['-role', 'hub'], expectedUrl: 'localhost:4444' + hubStatusAPIPath},
              {args: ['-role', 'hub', '-port', '12345'], expectedUrl: 'localhost:12345' + hubStatusAPIPath},
              {args: ['-role', 'hub', '-host', 'alias', '-port', '12345'], expectedUrl: 'alias:12345' + hubStatusAPIPath},
              {args: ['-role', 'hub', '-hub', 'https://foo/wd/register'], expectedUrl: 'localhost:4444' + hubStatusAPIPath},
              {args: ['-role', 'hub', '-hub', 'https://foo:6666/wd/register', '-port', '12345'], expectedUrl: 'localhost:12345' + hubStatusAPIPath},

              // Started as a Selenium Grid node
              {args: ['-role', 'node'], expectedUrl: 'localhost:5555' + nodeStatusAPIPath},
              {args: ['-role', 'node', '-port', '7777'], expectedUrl: 'localhost:7777' + nodeStatusAPIPath},
              {args: ['-role', 'node', '-host', 'alias', '-port', '7777'], expectedUrl: 'alias:7777' + nodeStatusAPIPath},
              {args: ['-role', 'node', '-hub', 'https://foo/wd/register'], expectedUrl: 'localhost:5555' + nodeStatusAPIPath},
              {args: ['-role', 'node', '-hub', 'https://foo:6666/wd/register', '-port', '7777'], expectedUrl: 'localhost:7777' + nodeStatusAPIPath},

              {args: ['-role', 'node', '-nodeConfig', path.join(__dirname, 'fixtures', 'config.node.json')], expectedUrl: 'foo:123' + nodeStatusAPIPath},
              {args: ['-role', 'node', '-host', 'alias', '-nodeConfig', path.join(__dirname, 'fixtures', 'config.node.json')], expectedUrl: 'alias:123' + nodeStatusAPIPath},
              {args: ['-role', 'node', '-host', 'alias', '-port', '7777', '-nodeConfig', path.join(__dirname, 'fixtures', 'config.node.json')], expectedUrl: 'alias:7777' + nodeStatusAPIPath},
            ];

  var testWithData = function (dataItem) {
    return function () {
      var actual = statusUrl.getSeleniumStatusUrl(dataItem.args);
      var expected = 'http://' + dataItem.expectedUrl;

      assert.equal(actual, expected);
    };
  };

  data.forEach(function (dataItem) {
    it('getSeleniumStatusUrl with seleniumArgs: ' + dataItem.args.join(' '), testWithData(dataItem));
  });
});
