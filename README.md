# ServicesStatusChecker

Simple site status checker with notifications configured for Slack.
By default, sends message only if any site is down. Use `-critical` to send any site status.

![demo](https://i.ibb.co/b5H8zg2/firefox-k-Abev-KSh-E6.png)

Requires `Resources/config.json`:
```json
{
	"token" : "APP-TOKEN-HERE",
	"webHookUrl" : "WEB-HOOK-URL", //obsolete, leave empty. WebHook is not able to customize sender's Name/Profile Pic.
	"teamId" : "TEAM-ID-HERE" //Team to mention in case any site is down.
}
```
and `Resources/sites.txt` which contains 1 URL per line.

