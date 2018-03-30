# 6.12.0 (2017-12-05)
* Update default selenium to 3.7.1 (#327)
* Update default IEDriver to 3.7.1 (#327)
* Update default geckodriver to 0.19.1 (#327)
* Update default Edge driver to 16299 (#330)
* Update downstream dependencies

# 6.11.0 (2017-10-20)
* Update default selenium to 3.6.0 (#310)
* Update default chromedriver to 2.33 (#310)
* Update default geckodriver to 0.19.0 (#310)
* Update default IEDriver to 3.6.0 (#310)
* Ability to install only one driver from default list (#313)

# 6.10.0 (2017-10-07)
* Fix selenium server status check URL when passing node config file (#308)
* Add timeout option to download request (#309)

# 6.9.0 (2017-09-04)
* Update default selenium to 3.5.3 (#302)
* Update default chromedriver to 2.32 (#302)

# 6.8.0 (2017-08-28)
* Update default selenium to 3.5.2 (#301)
* Update default IEDriver to 3.5.1 (#301)

# 6.7.0 (2017-08-15)
* Update default selenium to 3.5.0 (#296)
* Update default IEDriver to 3.5.0 (#296)
* Update default geckodriver to 0.18.0 (#295)

# 6.6.0 (2017-07-28)
* Update default chromedriver to 2.31 (#290)

# 6.5.0 (2017-06-08)

* Ability to install Microsoft Edge driver (#284)
* Update default chromedriver to 2.30 (#285)

# 6.4.1 (2017-04-24)

* Fix seleniumArgs handling in config file

# 6.4.0 (2017-04-24)

* Update selenium to 3.4.0
* Update IEDriver to 3.4.0
* Update geckodriver to 0.16.0

# 6.3.0 (2017-04-21)

* Update default chromedriver to 2.29 (#274)
* Update default IEDriver to 3.3.0 (#274)

# 6.2.0 (2017-04-05)

* Update default Selenium Server to 3.3.1 (#273)
* Update default Firefox driver to 0.15.0 (#273)
* Ability to collect debug logs for bug reports (#271)

# 6.1.0 (2017-03-17)

* Update default chromedriver to 2.28 (#267).

# 6.0.1 (2017-02-01)

* Fix default version for IEDriver to `3.0.0`.

# 6.0.0 (2017-01-29)

  * BREAKING CHANGE Using selenium 3.0.1 (Java 8 is needed)
  * Using yarn as package manager
  * Updated dependencies

# 5.11.2 (2017-01-26)

* Fix support for Node.JS v0.12

# 5.11.1 (2017-01-17)

* Update `request` and `mocha` dependencies.

# 5.11.0 (2017-01-17)

* update geckodriver to v0.13.0

# 5.10.0 (2017-01-16)

* update chromedriver version to 2.27
* --config file option

# 5.9.1 (2017-01-07)

* Fix incorrect download urls for Firefox drivers (#251)

# 5.9.0 (2016-12-04)

* update geckodriver to v0.11.1

# 5.8.0 (2016-10-26)

* update chrome driver to 2.25 #240
* add `opts.javaArgs` #234

# 5.7.2 (2016-09-22)

  * fix expectedRequest when less drivers given than usual

# 5.7.1 (2016-09-16)

# 5.7.0 (2016-09-16)

  * really fix selenium 3 install feature
  * add tests around computing the download urls
  * switch to new chromedriver 2.24

# 5.6.4 (2016-09-15)

  * Handle geckodriver/Marionette urls weirdness (#222)

# 5.6.3 (2016-09-09)
  * Support geckodriver v0.9.0 and v0.8.0 (6ef046448db058845aac709e839894ac95c65053)
# 5.6.2 (2016-08-29)
  * allow beta versions of selenium to be used #206
  * allow installing old chrome driver versions #213

# 5.6.1 (2016-08-23)
  * update download path for firefox driver 0.10.0 on mac

# 5.6.0 (2016-08-23)
  * upgrade chromedriver to 2.23
  * upgrade firefox driver to 0.10.0
  * better error message when java is possibly outdated

# 5.5.0 (2016-07-08)
  * add proxy option to programmatic interface

# 5.4.1 (2016-07-06)
  * fix install script when geckodriver downloaded

# 5.4.0 (2016-07-01)
  * update geckodriver to v0.9.0
  * update selenium to 2.53.1

# 5.3.1 (2016-06-28)

# 5.3.0 (2016-06-28)

* fix firefox win32 download url

# 5.2.0 (2016-06-28)

* add marionette driver
* bump chromedriver

# 5.1.1 (2016-06-10)

* Remove deprecated graceful-fs@3.x dependency

# 5.1.0 (2016-03-23)

* update to latest versions

# 5.0.0 (2016-02-24)

* BREAKING CHANGE when opts.drivers are passed on the programmatic interface,
do not merge all drivers option, only the one passed. So if you use {drivers: {chrome: {version: 42}}}, you will only get chrome version 42, not IE, not firefox

# 4.9.1 (2016-02-20)

* node 0.10 compat for fs.access

# 4.9.0 (2016-02-02)

  * feat(upgrades): upgrade default versions
    - Selenium version `2.50.1` and IE version `2.50.0` via http://www.seleniumhq.org/download/
    - Chromedriver version `2.21` via https://chromedriver.storage.googleapis.com

# 4.8.0 (2015-12-08)

  * feat(paths): support basePath option
  * feat(https): use https downloads by default

# 4.7.2 (2015-11-23)

  * fix(hub): selenium-standalone can now be run as hub

# 4.7.1 (2015-10-22)

  * fix(version): treat version as strings to allow 2.20 version, fixes #142

# 4.7.0 (2015-09-30)

  * feat(install): add `install --silent`

# 4.6.3 (2015-09-25)

  * fix require urijs

# 4.6.2 (2015-09-25)

  * update urijs package (renamed)

# 4.6.1 (2015-09-01)

  * release: do not include .selenium in package

# 4.6.0 (2015-09-01)

  * feat: selenium 2.47.1, ie driver 2.47.0, chrome 2.18
  * fix: fix default IE driver download url computation
  * fix: only append default stderr stdout listeners if not added by user

# 4.5.3 (2015-07-06)

  * feat: update to latest selenium/drivers version

# 4.4.3 (2015-07-06)

  * fix: error if already started

# 4.4.2 (2015-05-25)

  * fix: programmatic usage was broken

# 4.4.1 (2015-05-25)

  * fix: use isaacs/node-which instead of vvo/whereis to find JAVA path
    - fixes #96
  * fix: better handle selenium started event (when roles are hub or node)
    - fixes #98
    - fixes #97

# 4.4.0 (2015-04-17)

  * upgrade chrome driver to [2.15](https://chromedriver.storage.googleapis.com/2.15/notes.txt)

# 4.3.0 (2015-04-17)

  * parse selenium's -hub argument to find the hub hostname

# 4.2.2 (2015-03-23)

  * fix selenium binary start

# 4.2.1 (2015-03-20)

  * FIX: flush stderr/stdout of selenium so that it does not fails

# 4.2.0 (2015-03-02)

  * upgrade to selenium 2.45.0

# 4.1.0 (2015-02-06)

  * update chrome driver to [2.14](https://chromedriver.storage.googleapis.com/2.14/notes.txt)

# 4.0.0 (2015-02-06)

  * cache downloads, see #68

# 3.3.0 (2015-02-03)

  * forward request error to install error, #64

# 3.2.0 (2015-01-23)

  * add --baseURL, --drivers.*.baseURL options, also in programmatic API

# 3.1.2 (2015-01-17)

  * try to force npm to publish

# 3.1.1 (2015-01-17)

  * fixes #60, programmatic `install` without a `progressCb`

# 3.1.0 (2015-01-17)

  * add `opts.logger` to `install()`, defaults to `noop`
  * add `opts.progressCb` to `install(opts)`, now you can receive progress information
  * log more info when installing: source, destination
  * show progress when installing
  * check for pathsexistence before starting and error accordingly
  * add `opts.spawnCb` to `start()`, now you can receive the spawned process asap
  * more tests
  * readme tweaks

# 3.0.3 (2015-01-10)

  * inform user that `start-selenium` is deprecated

# 3.0.2 (2015-01-10)

  * ie fix

# 3.0.1 (2015-01-10)

  * ie fix

# 3.0.0 (2015-01-10)

  * complete refactoring
  * command line is now named `selenium-standalone`
  * you must use `selenium-standalone install` and then `selenium-standalone start`
  * programmatic API changed too, `require('selenium-standalone').install(cb)` or `require('selenium-standalone').start(cb)`
  * using the programmatic API, you must kill the server yourself, the child_process is sent in the `start` callback: `cb(err, cp)`
  * you can now install and start different selenium versions and drivers versions

# 2.44.0-7 (2015-01-04)

  * fix start-selenium when port is not `4444`

# 2.44.0-6 (2015-01-03)

  * add tests on new `cb()` functionnality
  * backward compat for people not using a `cb`
  * lower down callback loop to 200ms

# 2.44.0-5 (2015-01-03)

  * fix start-selenium command line (missing callback)

# 2.44.0-4 (2015-01-02)

  * programmatic API now exposes a callback to inform when selenium has started

# 2.44.0-3 (2015-01-02)

  * update chromedriver to [2.13](https://chromedriver.storage.googleapis.com/2.13/notes.txt)

# 2.44.0-2 (2015-01-02)

  * initial history generation
