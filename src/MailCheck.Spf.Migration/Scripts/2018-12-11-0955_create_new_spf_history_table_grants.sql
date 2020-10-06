GRANT SELECT, INSERT, UPDATE ON `spf_entity_history` TO '{env}-spf-ent' IDENTIFIED BY '{password}';
GRANT SELECT ON `spf_entity_history` TO '{env}-spf-api' IDENTIFIED BY '{password}';