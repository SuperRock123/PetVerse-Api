# TiDB Cloud - petverse database
# Server: gateway01.ap-southeast-1.prod.aws.tidbcloud.com:4000
# Database: petverse

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- comments
CREATE TABLE `comments` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `post_id` bigint unsigned NOT NULL,
  `user_id` bigint unsigned NOT NULL,
  `parent_id` bigint unsigned DEFAULT NULL COMMENT '父评论ID（二级评论）',
  `content` text NOT NULL,
  `likes_count` int unsigned NOT NULL DEFAULT '0',
  `status` tinyint NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`) /*T![clustered_index] CLUSTERED */,
  KEY `idx_post_id` (`post_id`),
  KEY `idx_parent_id` (`parent_id`),
  KEY `fk_2` (`user_id`),
  CONSTRAINT `fk_1` FOREIGN KEY (`post_id`) REFERENCES `posts` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_2` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_3` FOREIGN KEY (`parent_id`) REFERENCES `comments` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='动态评论表';

-- likes
CREATE TABLE `likes` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `target_type` varchar(32) NOT NULL COMMENT 'post / comment',
  `target_id` bigint unsigned NOT NULL,
  `user_id` bigint unsigned NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`) /*T![clustered_index] CLUSTERED */,
  UNIQUE KEY `uk_like` (`target_type`,`target_id`,`user_id`),
  KEY `idx_target` (`target_type`,`target_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='点赞记录';

-- media_resources
CREATE TABLE `media_resources` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `user_id` bigint unsigned NOT NULL COMMENT '上传用户ID（用于权限控制）',
  `media_type` tinyint NOT NULL COMMENT '0=图片 1=视频 2=音频 3=其他',
  `mime_type` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'image/jpeg, video/mp4, audio/mpeg 等',
  `original_name` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '原始文件名',
  `storage_key` varchar(512) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '对象存储 key，如 uploads/2026/02/27/uuid.jpg',
  `url_path` varchar(512) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '相对路径（可选）',
  `meta` json DEFAULT NULL COMMENT '{\n      "width": 1920,\n      "height": 1080,\n      "duration": 15.6,\n      "thumbnail_key": "thumbs/uuid_thumb.jpg",\n      "size_bytes": 2846721,\n      "alt": "描述文本"\n    }',
  `status` tinyint NOT NULL DEFAULT '1' COMMENT '1=正常 0=删除 -1=处理中',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`) /*T![clustered_index] CLUSTERED */,
  KEY `idx_user_id` (`user_id`),
  KEY `idx_media_type` (`media_type`),
  KEY `idx_status` (`status`),
  UNIQUE KEY `storage_key` (`storage_key`),
  CONSTRAINT `fk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci AUTO_INCREMENT=60001 COMMENT='统一媒体资源表（被帖子等引用）';

-- pet_vaccines
CREATE TABLE `pet_vaccines` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `pet_id` bigint unsigned NOT NULL,
  `vaccine_name` varchar(128) NOT NULL,
  `vaccinate_date` date NOT NULL,
  `next_date` date DEFAULT NULL,
  `hospital` varchar(128) DEFAULT NULL,
  `remark` text DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`) /*T![clustered_index] CLUSTERED */,
  KEY `idx_pet_id` (`pet_id`),
  CONSTRAINT `fk_1` FOREIGN KEY (`pet_id`) REFERENCES `pets` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='宠物疫苗记录';

-- pets
CREATE TABLE `pets` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `user_id` bigint unsigned NOT NULL,
  `name` varchar(64) NOT NULL COMMENT '宠物名',
  `breed` varchar(128) DEFAULT NULL COMMENT '品种',
  `gender` tinyint DEFAULT NULL COMMENT '0=未知 1=公 2=母',
  `birthday` date DEFAULT NULL,
  `weight_kg` decimal(5,2) DEFAULT NULL,
  `health_status` varchar(64) DEFAULT NULL COMMENT '健康状态描述',
  `avatar_url` varchar(512) DEFAULT NULL COMMENT '宠物头像(object_key)',
  `pettag_id` varchar(64) DEFAULT NULL COMMENT '绑定的Pettag序列号（Phase1占位）',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`) /*T![clustered_index] CLUSTERED */,
  KEY `idx_user_id` (`user_id`),
  CONSTRAINT `fk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin AUTO_INCREMENT=30001 COMMENT='宠物档案';

-- pettags
CREATE TABLE `pettags` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `serial_number` varchar(64) NOT NULL COMMENT '设备序列号',
  `user_id` bigint unsigned DEFAULT NULL,
  `pet_id` bigint unsigned DEFAULT NULL,
  `status` tinyint DEFAULT '0' COMMENT '0=未激活 1=在线 2=离线',
  `battery_level` tinyint DEFAULT NULL,
  `last_seen` datetime DEFAULT NULL,
  `firmware_version` varchar(32) DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`) /*T![clustered_index] CLUSTERED */,
  UNIQUE KEY `uk_serial` (`serial_number`),
  KEY `idx_user_pet` (`user_id`,`pet_id`),
  UNIQUE KEY `serial_number` (`serial_number`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='Pettag 硬件设备表';

-- posts
CREATE TABLE `posts` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `user_id` bigint unsigned NOT NULL,
  `pet_id` bigint unsigned DEFAULT NULL COMMENT '关联宠物（可选）',
  `content` text COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '文字内容（可考虑后续升级为 Markdown）',
  `media_ids` json DEFAULT NULL COMMENT '关联媒体ID数组，如 [1,2,3]',
  `location` varchar(128) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `likes_count` int unsigned NOT NULL DEFAULT '0',
  `comments_count` int unsigned NOT NULL DEFAULT '0',
  `view_count` int unsigned NOT NULL DEFAULT '0',
  `media_count` tinyint unsigned GENERATED ALWAYS AS (json_length(`media_ids`)) STORED COMMENT '媒体数量（自动计算）',
  `visibility` tinyint NOT NULL DEFAULT '1' COMMENT '1=公开 2=仅好友 3=私密',
  `status` tinyint NOT NULL DEFAULT '1' COMMENT '1=正常 0=已删除 -1=审核中 -2=已屏蔽',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `published_at` datetime DEFAULT NULL COMMENT '发布时间（支持延迟发布，可选）',
  PRIMARY KEY (`id`) /*T![clustered_index] CLUSTERED */,
  KEY `idx_user_id` (`user_id`),
  KEY `idx_pet_id` (`pet_id`),
  KEY `idx_created_at` (`created_at`),
  KEY `idx_status_visibility` (`status`,`visibility`),
  FULLTEXT INDEX `ft_content`(`content`) WITH PARSER STANDARD COMMENT '支持内容搜索',
  CONSTRAINT `fk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_2` FOREIGN KEY (`pet_id`) REFERENCES `pets` (`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci AUTO_INCREMENT=30001 COMMENT='社区动态主表（媒体统一引用）';

-- reports
CREATE TABLE `reports` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `reporter_id` bigint unsigned NOT NULL,
  `target_type` varchar(32) NOT NULL COMMENT 'post / comment / user',
  `target_id` bigint unsigned NOT NULL,
  `reason_type` tinyint NOT NULL COMMENT '1=色情 2=暴力 3=广告 ...',
  `reason_detail` text DEFAULT NULL,
  `status` tinyint NOT NULL DEFAULT '0' COMMENT '0=待处理 1=已处理 2=忽略 -1=无效',
  `handled_by` bigint unsigned DEFAULT NULL,
  `handled_at` datetime DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`) /*T![clustered_index] CLUSTERED */,
  KEY `idx_target` (`target_type`,`target_id`),
  KEY `idx_status` (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='举报表';

-- system_configs
CREATE TABLE `system_configs` (
  `key` varchar(128) NOT NULL,
  `value` text NOT NULL,
  `description` varchar(255) DEFAULT NULL,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`key`) /*T![clustered_index] CLUSTERED */
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='系统配置表';

-- user_tag_relations
CREATE TABLE `user_tag_relations` (
  `user_id` bigint unsigned NOT NULL,
  `tag_id` bigint unsigned NOT NULL,
  `assigned_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `assigned_by` bigint unsigned DEFAULT NULL COMMENT '分配人（admin或系统）',
  PRIMARY KEY (`user_id`,`tag_id`) /*T![clustered_index] CLUSTERED */,
  KEY `fk_2` (`tag_id`),
  CONSTRAINT `fk_1` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `fk_2` FOREIGN KEY (`tag_id`) REFERENCES `user_tags` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='用户-标签关联表';

-- user_tags
CREATE TABLE `user_tags` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `tag_name` varchar(32) NOT NULL COMMENT '标签名称，如：专业训犬师',
  `description` varchar(255) DEFAULT NULL,
  `color` varchar(16) DEFAULT NULL COMMENT '展示颜色 #FF7733',
  `icon_url` varchar(512) DEFAULT NULL,
  `extra_info` json DEFAULT NULL COMMENT '扩展信息（如权限标识）',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`) /*T![clustered_index] CLUSTERED */,
  UNIQUE KEY `uk_tag_name` (`tag_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_bin COMMENT='用户身份标签定义表';

-- users
CREATE TABLE `users` (
  `id` bigint unsigned NOT NULL AUTO_INCREMENT COMMENT '用户ID',
  `username` varchar(64) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT '用户名（可为空，手机号/邮箱登录时可不填）',
  `nickname` varchar(64) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '昵称',
  `phone` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '手机号',
  `email` varchar(128) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '邮箱',
  `password_hash` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '密码（bcrypt/argon2）',
  `avatar_url` varchar(512) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '用户头像(object_key)',
  `bio` text COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT '个人简介',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `last_login_at` datetime DEFAULT NULL,
  `status` tinyint NOT NULL DEFAULT '1' COMMENT '1=正常 0=禁用 -1=注销',
  PRIMARY KEY (`id`) /*T![clustered_index] CLUSTERED */,
  UNIQUE KEY `uk_phone` (`phone`),
  UNIQUE KEY `uk_email` (`email`),
  KEY `idx_status` (`status`),
  UNIQUE KEY `username` (`username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci AUTO_INCREMENT=30001 COMMENT='用户表';

SET FOREIGN_KEY_CHECKS = 1;
