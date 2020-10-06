GRANT SELECT, INSERT, UPDATE, DELETE ON `spf_entity` TO '{env}-spf-ent' IDENTIFIED BY '{password}';

GRANT SELECT ON `spf_entity` TO '{env}-spf-api' IDENTIFIED BY '{password}';
GRANT SELECT ON `spf_entity_history` TO '{env}-spf-api' IDENTIFIED BY '{password}';

GRANT SELECT, INSERT, UPDATE ON `spf_entity_history` TO '{env}-spf-ent' IDENTIFIED BY '{password}';

GRANT SELECT, INSERT, UPDATE, DELETE ON `spf_scheduled_records` TO '{env}-spf-sch' IDENTIFIED BY '{password}';

GRANT SELECT ON `spf_entity` TO '{env}_reports' IDENTIFIED BY '{password}';
GRANT SELECT INTO S3 ON *.* TO '{env}_reports' IDENTIFIED BY '{password}';