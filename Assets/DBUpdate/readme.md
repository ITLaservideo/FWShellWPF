# DBUpdate

SQL update scripts applied automatically at startup by `SQL.EnsureTablesAndDataExist()`.

## How it works

- Files must be named with a numeric prefix: `1.Description.sql`, `42.AddTable.sql`, etc.
- The current version is stored in `TG_LocalSettings` under the key `db_update_version`.
- At startup, all scripts whose numeric prefix is **greater than** (>) the `db_update_version` are executed in ascending order.
- After all scripts run, `db_update_version` is updated to the highest numeric prefix.
- To split a single file into multiple SQL batches, separate them with the token:
    ```sql
    --#split-sql-batch#--
    ```
- Errors inside a script are **caught and ignored** — startup is never blocked by a failed migration. Check logs if a script silently fails.
