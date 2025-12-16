# AI Agent Instructions

## Issue Tracking with Beads

This project uses [Beads](https://github.com/modernice/beads) for issue tracking. When working on tasks:

### Creating Issues

- Use `bd create` to create new issues instead of maintaining markdown TODO lists
- Include clear titles and descriptions
- Set appropriate priority levels (0 = high, 1 = medium, 2 = low)
- Add relevant labels (e.g., "bug", "feature", "refactoring", "DRY")

### Working with Issues

- Use `bd list` to see all open issues
- Use `bd show <issue-id>` to view issue details
- Use `bd update <issue-id>` to update issue status or fields
- Use `bd close <issue-id>` when work is complete
- Use `bd comment <issue-id>` to add notes or updates

### Issue Workflow

1. Query issues: `bd list` or `bd ready` for ready-to-work items
2. Start work on an issue (consider updating status to "in-progress")
3. Complete the implementation and tests
4. Close the issue: `bd close <issue-id>`
5. Commit changes with issue reference in commit message

### Commands Reference

```bash
bd list                    # List all issues
bd ready                   # Show issues ready to work (no blockers)
bd create                  # Create new issue
bd show <issue-id>         # Show issue details
bd close <issue-id>        # Close an issue
bd comment <issue-id>      # Add comment to issue
bd update <issue-id>       # Update issue fields
bd search <query>          # Search issues by text
bd status                  # Show database overview
```

### Benefits

- Issues are tracked in git with the codebase
- No external service dependencies
- Full command-line workflow integration
- Issues can have dependencies and relationships
- Better than markdown for collaborative and long-term tracking
