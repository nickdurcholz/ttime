# TTime

TTime is a CLI time tracker intended for people who need to track the time they spend on various tasks throughout the day. You use the `ttime start ` command to start tracking time that is optionally identified by a number of tags. Use `ttime stop` to end tracking simply start a different task using `ttime start` without issuing a stop command.

## Configuration settings
TTIME_DATA environment can be set to override the default path to the data file, which is ~/.ttime/data.litedb for mac and unix and <ApplicationData>\ttime\data.litedb on Windows.

All other configuration settings are stored in the ttime data file.  If a data file does not exist, one will be created for you on startup.

### See a list of configuration settings
> `ttime config`

### Configuration Settings
> `ttime config rounding <number-of-minutes>`

Causes ttime to round to the nearest number of minutes when reporting. This does not impact internal calculations; it only impacts the final result, so using this setting can lead to discrepencies if you sum the totals from multiple reports. For example, if you get a report from monday and a report from tuesday, and total the times together, it may be slightly different than the total if you had just request a combined report for monday and tuesday.

For example, to round all numbers reported to the nearest quarter-hour, run `ttime config rounding 15`

Defaults to zero, which means to not round.

> `ttime config defaultReportPeriod <day-of-week | last-week | yesterday | today>`

Sets the default action when you type `ttime report`. Defaults to yesterday.

> `ttime config defaultFormat <text|csv|xml|json>`

Defaults to `text`

> `ttime config startOfWeek <day-of-week>`

First day of the week. Defaults to `Monday`

## Usage
Usage help and examples are provided by the tool. To see a list of available commands, type the following:

> `ttime help`

More information about a specific command can be obtained with

> `ttime help command`

### Examples:

Command | Effect
--- | ---
`ttime start <task>` | Start tracking time on the given task
`ttime stop` | Stop tracking time. This is a bit like "clocking out"
`ttime report` | Show report of time worked for the default reporting period
`ttime report yesterday` | Show report for yesterday starting at 12:00 AM and stopping at 11:59:59 PM
`ttime report last-week` | Show report for the previous full week
`ttime report from=2019-1-25T9:00 issue-707` | Show how much time you have spent on entries tagged with issue-707 starting at 9 AM on January 25 2019 until until now