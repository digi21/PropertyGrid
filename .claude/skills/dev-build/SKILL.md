---
name: dev-build
description: Build, verify and publish a Digi21.WinUI.PropertyGrid development package to the local feed. Use whenever a change is finished and the consumer needs it — "publish", "pack", "saca una dev", "dame la versión". Covers the full sequence including the visual check and the probe, and the tag rule that decides whether nuget.org gets involved.
---

# Publishing a development build

The library is consumed from a local feed by an application on another machine, through a second
Claude Code instance that reports back. Getting a build to it is this sequence, in this order.

## 1. Build and test

```
dotnet build -c Release
dotnet test -c Release --no-build
```

Both have to be clean. Warnings are errors here, and missing XML documentation on a public member is
a warning.

## 2. Look at it

A property grid is a visual control and the test suite cannot see it. Take the picture and **open
it**:

```
dotnet run --project samples/PropertyGridGallery -- --screenshot assets/gallery.png light
dotnet run --project samples/PropertyGridGallery -- --screenshot assets/gallery-dark.png dark
```

Read the PNG. Twice in this project a change built, passed every test, and was visibly broken —
names in the wrong theme's colour, a swatch that never painted. Refresh both images when anything
visual changed; they are the README's.

To aim the camera at particular rows, temporarily set `Grid.FilterText` in
`OpenNestedRowForPicture`, capture, and put it back.

## 3. Measure, if rows changed

If anything touched how rows are realized, recycled or sized, run the probe — it lives in a separate
repository next door, deliberately not published:

```
dotnet run --project ../PropertyGridProbe -- --diagnose report.txt
```

Check: row heights, that recycling still reuses elements, and above all **that no writes reached the
model while nobody was typing**. That last one has to be zero.

## 4. Update the record

- `CHANGELOG.md`, under `[Unreleased]` — add the heading if the last one is a released version.
  Fixes go under `### Fixed` with what the symptom was, not just what changed.
- `docs/winui.md` if a WinUI trap was involved. That file exists so nobody pays for the same one
  twice.
- The relevant `docs/` page if public API changed.

## 5. Commit and pack

Commits are in English, conventional style, with a body explaining the reasoning. They are signed —
never pass `--no-gpg-sign`.

```
git add -A
git commit -S -m "..."
dotnet pack src/Digi21.WinUI.PropertyGrid/Digi21.WinUI.PropertyGrid.csproj -c Release -o C:\LocalNuGet
git push
```

MinVer works the version out from the git history: after `v1.0.0` it produces `1.0.1-dev.N`, where N
counts commits since the tag. **Every pack therefore produces a version nobody has used**, which is
the whole point — the consumer picks it up without anyone clearing the NuGet cache.

Read the version back off the `Successfully created package` line and tell the user. Do not guess
it: N is commits, not builds, so it jumps.

## 6. The tag rule

**Do not create or push a `v*` tag.** That is what triggers the release workflow and publishes to
nuget.org, and nothing should be published until the consumer confirms the integration. There are no
tags in this repository yet, on purpose.

When the time comes it is `git tag v0.1.0 && git push origin v0.1.0`, and it needs the `NUGET_USER`
secret set in the repository — the nuget.org profile name, not the email.

## Reporting back

The consumer is a different Claude Code instance on a different machine, so it cannot read this
session. Give the user a message they can paste, containing:

- the exact version;
- what changed, and for a fix, what the actual cause turned out to be — they usually sent a
  hypothesis and deserve to know whether it was right;
- **any breaking API change**, spelled out;
- what to re-check on their bench.

Their local feed is `F:\source\digi21\Librerias para compilar Digi3D.NET`, which is not reachable
from this machine. The package goes to `C:\LocalNuGet` and the user copies it.
