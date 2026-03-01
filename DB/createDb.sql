# localhost:3306 root 123456
create database petverse;
use petverse;

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;
create table __efmigrationshistory
(
    MigrationId    varchar(150) not null
        primary key,
    ProductVersion varchar(32)  not null
);

create table system_configs
(
    `key`       varchar(128) not null
        primary key,
    value       longtext     not null,
    description varchar(255) null,
    updated_at  datetime(6)  not null
);

create table user_tags
(
    id          bigint unsigned auto_increment
        primary key,
    tag_name    varchar(32)  not null,
    description varchar(255) null,
    color       varchar(16)  null,
    icon_url    varchar(512) null,
    extra_info  longtext     null,
    created_at  datetime(6)  not null,
    constraint IX_user_tags_tag_name
        unique (tag_name)
);

create table users
(
    id            bigint unsigned auto_increment
        primary key,
    username      varchar(64)       not null,
    nickname      varchar(64)       null,
    phone         varchar(20)       null,
    email         varchar(128)      null,
    password_hash varchar(255)      null,
    avatar_url    varchar(512)      null,
    bio           longtext          null,
    created_at    datetime(6)       not null,
    updated_at    datetime(6)       not null,
    last_login_at datetime(6)       null,
    status        tinyint default 1 not null,
    constraint IX_users_email
        unique (email),
    constraint IX_users_phone
        unique (phone)
);

create table pets
(
    id            bigint unsigned auto_increment
        primary key,
    user_id       bigint unsigned not null,
    name          varchar(64)     not null,
    breed         varchar(128)    null,
    gender        tinyint         null,
    birthday      date            null,
    weight_kg     decimal(65, 30) null,
    health_status varchar(64)     null,
    avatar_url    varchar(512)    null,
    pettag_id     varchar(64)     null,
    created_at    datetime(6)     not null,
    updated_at    datetime(6)     not null,
    constraint FK_pets_users_user_id
        foreign key (user_id) references users (id)
            on delete cascade
);

create table pet_vaccines
(
    id             bigint unsigned auto_increment
        primary key,
    pet_id         bigint unsigned not null,
    vaccine_name   varchar(128)    not null,
    vaccinate_date date            not null,
    next_date      date            null,
    hospital       varchar(128)    null,
    remark         longtext        null,
    created_at     datetime(6)     not null,
    constraint FK_pet_vaccines_pets_pet_id
        foreign key (pet_id) references pets (id)
            on delete cascade
);

create index IX_pet_vaccines_pet_id
    on pet_vaccines (pet_id);

create index IX_pets_user_id
    on pets (user_id);

create table pettags
(
    id               bigint unsigned auto_increment
        primary key,
    serial_number    varchar(64)       not null,
    user_id          bigint unsigned   null,
    pet_id           bigint unsigned   null,
    status           tinyint default 0 not null,
    battery_level    tinyint           null,
    last_seen        datetime(6)       null,
    firmware_version varchar(32)       null,
    created_at       datetime(6)       not null,
    constraint IX_pettags_serial_number
        unique (serial_number),
    constraint FK_pettags_pets_pet_id
        foreign key (pet_id) references pets (id)
            on delete set null,
    constraint FK_pettags_users_user_id
        foreign key (user_id) references users (id)
            on delete set null
);

create index IX_pettags_pet_id
    on pettags (pet_id);

create index IX_pettags_user_id_pet_id
    on pettags (user_id, pet_id);

create table posts
(
    id             bigint unsigned auto_increment
        primary key,
    user_id        bigint unsigned              not null,
    pet_id         bigint unsigned              null,
    content        longtext                     not null,
    location       varchar(128)                 null,
    likes_count    int unsigned     default '0' not null,
    comments_count int unsigned     default '0' not null,
    visibility     tinyint          default 1   not null,
    status         tinyint          default 1   not null,
    created_at     datetime(6)                  not null,
    updated_at     datetime(6)                  not null,
    media_count    tinyint unsigned default '0' not null,
    published_at   datetime(6)                  null,
    view_count     int unsigned     default '0' not null,
    constraint FK_posts_pets_pet_id
        foreign key (pet_id) references pets (id)
            on delete set null,
    constraint FK_posts_users_user_id
        foreign key (user_id) references users (id)
            on delete cascade
);

create table comments
(
    id          bigint unsigned auto_increment
        primary key,
    post_id     bigint unsigned          not null,
    user_id     bigint unsigned          not null,
    parent_id   bigint unsigned          null,
    content     longtext                 not null,
    likes_count int unsigned default '0' not null,
    status      tinyint      default 1   not null,
    created_at  datetime(6)              not null,
    constraint FK_comments_comments_parent_id
        foreign key (parent_id) references comments (id)
            on delete cascade,
    constraint FK_comments_posts_post_id
        foreign key (post_id) references posts (id)
            on delete cascade,
    constraint FK_comments_users_user_id
        foreign key (user_id) references users (id)
            on delete cascade
);

create index IX_comments_parent_id
    on comments (parent_id);

create index IX_comments_post_id
    on comments (post_id);

create index IX_comments_user_id
    on comments (user_id);

create table likes
(
    id          bigint unsigned auto_increment
        primary key,
    target_type varchar(32)     not null,
    target_id   bigint unsigned not null,
    user_id     bigint unsigned not null,
    created_at  datetime(6)     not null,
    CommentId   bigint unsigned null,
    PostId      bigint unsigned null,
    constraint IX_likes_target_type_target_id_user_id
        unique (target_type, target_id, user_id),
    constraint FK_likes_comments_CommentId
        foreign key (CommentId) references comments (id),
    constraint FK_likes_posts_PostId
        foreign key (PostId) references posts (id),
    constraint FK_likes_users_user_id
        foreign key (user_id) references users (id)
            on delete cascade
);

create index IX_likes_CommentId
    on likes (CommentId);

create index IX_likes_PostId
    on likes (PostId);

create index IX_likes_target_type_target_id
    on likes (target_type, target_id);

create index IX_likes_user_id
    on likes (user_id);

create table post_media
(
    id            bigint unsigned auto_increment
        primary key,
    post_id       bigint unsigned               not null,
    media_type    int                           not null,
    mime_type     varchar(100)                  not null,
    original_name varchar(255)                  null,
    storage_key   varchar(512)                  not null,
    url_path      varchar(512)                  null,
    meta          longtext                      null,
    display_order smallint unsigned default '0' not null,
    status        tinyint           default 1   not null,
    created_at    datetime(6)                   not null,
    updated_at    datetime(6)                   not null,
    constraint IX_post_media_post_id_storage_key
        unique (post_id, storage_key),
    constraint FK_post_media_posts_post_id
        foreign key (post_id) references posts (id)
            on delete cascade
);

create index IX_post_media_media_type
    on post_media (media_type);

create index IX_post_media_post_id
    on post_media (post_id);

create index IX_posts_created_at
    on posts (created_at);

create index IX_posts_pet_id
    on posts (pet_id);

create index IX_posts_status_visibility
    on posts (status, visibility);

create index IX_posts_user_id
    on posts (user_id);

create table reports
(
    id            bigint unsigned auto_increment
        primary key,
    reporter_id   bigint unsigned   not null,
    target_type   varchar(32)       not null,
    target_id     bigint unsigned   not null,
    reason_type   tinyint           not null,
    reason_detail longtext          null,
    status        tinyint default 0 not null,
    handled_by    bigint unsigned   null,
    handled_at    datetime(6)       null,
    created_at    datetime(6)       not null,
    constraint FK_reports_users_handled_by
        foreign key (handled_by) references users (id)
            on delete set null,
    constraint FK_reports_users_reporter_id
        foreign key (reporter_id) references users (id)
            on delete cascade
);

create index IX_reports_handled_by
    on reports (handled_by);

create index IX_reports_reporter_id
    on reports (reporter_id);

create index IX_reports_status
    on reports (status);

create index IX_reports_target_type_target_id
    on reports (target_type, target_id);

create table user_tag_relations
(
    user_id     bigint unsigned not null,
    tag_id      bigint unsigned not null,
    assigned_at datetime(6)     not null,
    assigned_by bigint unsigned null,
    primary key (user_id, tag_id),
    constraint FK_user_tag_relations_user_tags_tag_id
        foreign key (tag_id) references user_tags (id)
            on delete cascade,
    constraint FK_user_tag_relations_users_user_id
        foreign key (user_id) references users (id)
            on delete cascade
);

create index IX_user_tag_relations_tag_id
    on user_tag_relations (tag_id);

create index IX_users_status
    on users (status);


