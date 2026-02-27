# localhost:3306 root 123456
create database petverse;
use petverse;

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- 用户表
CREATE TABLE `users` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT COMMENT '用户ID',
  `username` VARCHAR(64) NOT NULL UNIQUE COMMENT '用户名（可为空，手机号/邮箱登录时可不填）',
  `nickname` VARCHAR(64) DEFAULT NULL COMMENT '昵称',
  `phone` VARCHAR(20) DEFAULT NULL COMMENT '手机号',
  `email` VARCHAR(128) DEFAULT NULL COMMENT '邮箱',
  `password_hash` VARCHAR(255) DEFAULT NULL COMMENT '密码（bcrypt/argon2）',
  `avatar_url` VARCHAR(512) DEFAULT NULL COMMENT '用户头像',
  `bio` TEXT COMMENT '个人简介',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  `last_login_at` DATETIME DEFAULT NULL,
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '1=正常 0=禁用 -1=注销',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_phone` (`phone`),
  UNIQUE KEY `uk_email` (`email`),
  INDEX `idx_status` (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='用户表';

-- 用户标签（动态身份标签）
CREATE TABLE `user_tags` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `tag_name` VARCHAR(32) NOT NULL COMMENT '标签名称，如：专业训犬师',
  `description` VARCHAR(255) DEFAULT NULL,
  `color` VARCHAR(16) DEFAULT NULL COMMENT '展示颜色 #FF7733',
  `icon_url` VARCHAR(512) DEFAULT NULL,
  `extra_info` JSON DEFAULT NULL COMMENT '扩展信息（如权限标识）',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_tag_name` (`tag_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='用户身份标签定义表';

CREATE TABLE `user_tag_relations` (
  `user_id` BIGINT UNSIGNED NOT NULL,
  `tag_id` BIGINT UNSIGNED NOT NULL,
  `assigned_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `assigned_by` BIGINT UNSIGNED DEFAULT NULL COMMENT '分配人（admin或系统）',
  PRIMARY KEY (`user_id`, `tag_id`),
  FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE,
  FOREIGN KEY (`tag_id`) REFERENCES `user_tags`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='用户-标签关联表';

-- 宠物表
CREATE TABLE `pets` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` BIGINT UNSIGNED NOT NULL,
  `name` VARCHAR(64) NOT NULL COMMENT '宠物名',
  `breed` VARCHAR(128) DEFAULT NULL COMMENT '品种',
  `gender` TINYINT DEFAULT NULL COMMENT '0=未知 1=公 2=母',
  `birthday` DATE DEFAULT NULL,
  `weight_kg` DECIMAL(5,2) DEFAULT NULL,
  `health_status` VARCHAR(64) DEFAULT NULL COMMENT '健康状态描述',
  `avatar_url` VARCHAR(512) DEFAULT NULL COMMENT '宠物头像',
  `pettag_id` VARCHAR(64) DEFAULT NULL COMMENT '绑定的Pettag序列号（Phase1占位）',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  INDEX `idx_user_id` (`user_id`),
  FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='宠物档案';

-- 宠物疫苗记录
CREATE TABLE `pet_vaccines` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `pet_id` BIGINT UNSIGNED NOT NULL,
  `vaccine_name` VARCHAR(128) NOT NULL,
  `vaccinate_date` DATE NOT NULL,
  `next_date` DATE DEFAULT NULL,
  `hospital` VARCHAR(128) DEFAULT NULL,
  `remark` TEXT,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  INDEX `idx_pet_id` (`pet_id`),
  FOREIGN KEY (`pet_id`) REFERENCES `pets`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='宠物疫苗记录';

-- 社区动态（主贴）
CREATE TABLE `posts` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `user_id` BIGINT UNSIGNED NOT NULL,
  `pet_id` BIGINT UNSIGNED DEFAULT NULL COMMENT '关联宠物（可选）',
  `content` TEXT NOT NULL COMMENT '文字内容',
  `media_urls` JSON DEFAULT NULL COMMENT '图片/视频地址列表',
  `location` VARCHAR(128) DEFAULT NULL,
  `likes_count` INT UNSIGNED NOT NULL DEFAULT 0,
  `comments_count` INT UNSIGNED NOT NULL DEFAULT 0,
  `visibility` TINYINT NOT NULL DEFAULT 1 COMMENT '1=公开 2=仅好友 3=私密',
  `status` TINYINT NOT NULL DEFAULT 1 COMMENT '1=正常 0=已删除 -1=审核中 -2=已屏蔽',
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  INDEX `idx_user_id` (`user_id`),
  INDEX `idx_pet_id` (`pet_id`),
  INDEX `idx_created_at` (`created_at`),
  FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE,
  FOREIGN KEY (`pet_id`) REFERENCES `pets`(`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='社区动态主表';

-- 评论（支持二级评论）
CREATE TABLE `comments` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `post_id` BIGINT UNSIGNED NOT NULL,
  `user_id` BIGINT UNSIGNED NOT NULL,
  `parent_id` BIGINT UNSIGNED DEFAULT NULL COMMENT '父评论ID（二级评论）',
  `content` TEXT NOT NULL,
  `likes_count` INT UNSIGNED NOT NULL DEFAULT 0,
  `status` TINYINT NOT NULL DEFAULT 1,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  INDEX `idx_post_id` (`post_id`),
  INDEX `idx_parent_id` (`parent_id`),
  FOREIGN KEY (`post_id`) REFERENCES `posts`(`id`) ON DELETE CASCADE,
  FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE,
  FOREIGN KEY (`parent_id`) REFERENCES `comments`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='动态评论表';

-- 点赞记录（防重复点赞）
CREATE TABLE `likes` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `target_type` VARCHAR(32) NOT NULL COMMENT 'post / comment',
  `target_id` BIGINT UNSIGNED NOT NULL,
  `user_id` BIGINT UNSIGNED NOT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_like` (`target_type`, `target_id`, `user_id`),
  INDEX `idx_target` (`target_type`, `target_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='点赞记录';

-- 举报记录
CREATE TABLE `reports` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `reporter_id` BIGINT UNSIGNED NOT NULL,
  `target_type` VARCHAR(32) NOT NULL COMMENT 'post / comment / user',
  `target_id` BIGINT UNSIGNED NOT NULL,
  `reason_type` TINYINT NOT NULL COMMENT '1=色情 2=暴力 3=广告 ...',
  `reason_detail` TEXT,
  `status` TINYINT NOT NULL DEFAULT 0 COMMENT '0=待处理 1=已处理 2=忽略 -1=无效',
  `handled_by` BIGINT UNSIGNED DEFAULT NULL,
  `handled_at` DATETIME DEFAULT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  INDEX `idx_target` (`target_type`, `target_id`),
  INDEX `idx_status` (`status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='举报表';

-- Pettag 设备（Phase1 占位，后续扩展）
CREATE TABLE `pettags` (
  `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  `serial_number` VARCHAR(64) NOT NULL UNIQUE COMMENT '设备序列号',
  `user_id` BIGINT UNSIGNED DEFAULT NULL,
  `pet_id` BIGINT UNSIGNED DEFAULT NULL,
  `status` TINYINT DEFAULT 0 COMMENT '0=未激活 1=在线 2=离线',
  `battery_level` TINYINT DEFAULT NULL,
  `last_seen` DATETIME DEFAULT NULL,
  `firmware_version` VARCHAR(32) DEFAULT NULL,
  `created_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uk_serial` (`serial_number`),
  INDEX `idx_user_pet` (`user_id`, `pet_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='Pettag 硬件设备表';

-- 系统配置（可选，方便后期管理后台修改）
CREATE TABLE `system_configs` (
  `key` VARCHAR(128) NOT NULL PRIMARY KEY,
  `value` TEXT NOT NULL,
  `description` VARCHAR(255) DEFAULT NULL,
  `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='系统配置表';
