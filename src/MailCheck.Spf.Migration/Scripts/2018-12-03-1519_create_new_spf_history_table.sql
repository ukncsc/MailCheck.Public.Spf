ALTER TABLE `spf_entity_history` RENAME `spf_entity_history_old`;

CREATE TABLE `spf_entity_history` (
  `id` VARCHAR(255) NOT NULL,
  `state` JSON NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;