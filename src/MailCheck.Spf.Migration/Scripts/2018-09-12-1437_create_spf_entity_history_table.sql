CREATE TABLE `spf_entity_history` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `entity_id` varchar(255) NOT NULL,
  `state` json NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;