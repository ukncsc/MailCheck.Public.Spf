UPDATE spf_entity
SET 
id = LOWER(id),
state = JSON_SET(state, '$.id', LOWER(state->>'$.id'))
WHERE id REGEXP BINARY '[A-Z]';

UPDATE spf_entity_history
SET 
id = LOWER(id),
state = JSON_SET(state, '$.id', LOWER(state->>'$.id'))
WHERE id REGEXP BINARY '[A-Z]';