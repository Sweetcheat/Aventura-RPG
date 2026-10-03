\# Aventura RPG — Agent Instructions



\## General

\- Make the smallest change necessary to satisfy the user's request.

\- Follow YAGNI.

\- Do not introduce unnecessary abstractions, dependencies, files, or refactors.

\- Do not modify unrelated code.

\- Preserve existing behavior unless the task explicitly requires changing it.

\- Do not fix unrelated issues discovered during a task unless explicitly authorized.



\## Before Changes

\- Inspect the relevant code before modifying it.

\- Clearly identify which files and areas need to change.

\- If the requested approach has a significant technical problem, stop and explain it instead of silently expanding the scope.



\## Build and Testing

\- After implementation, review the complete diff.

\- Build the solution when applicable.

\- Run relevant tests or practical verification.

\- Report build/test results.

\- Do not consider a task complete if the build fails.



\## Git

\- Never discard user changes.

\- Never use `git reset --hard`.

\- Never use destructive Git commands unless explicitly authorized.

\- Never use `git push --force`.

\- Never push automatically.

\- Only push when the user explicitly authorizes the push.

\- Before committing, review the diff and verify that only task-related changes are included.

\- Before pushing, run `git status` and verify the working tree is clean.

\- The local working branch is `main`.

\- The primary GitHub branch is `master`.

\- When the user explicitly authorizes a push, push with:

&#x20; git push origin main:master



\## Commit

\- Only create a commit when the user explicitly requests a commit or explicitly authorizes commit as part of the current task.

\- Use a concise, descriptive commit message.

\- After committing, report the commit hash and summary.



\## Push

When the user explicitly authorizes a push:

1\. Run `git status`.

2\. Verify the working tree is clean.

3\. Verify the current branch is `main`.

4\. Verify the commit to be pushed is the intended HEAD.

5\. Push using `git push origin main:master`.

6\. Report the push result.

7\. If the push fails, stop and report the error. Do not attempt force push or unrelated Git recovery.



\## Scope Control

\- Do not make additional improvements merely because they are possible.

\- Do not perform broad modernization or architectural refactoring unless explicitly requested.

\- Prefer focused, reversible changes.

