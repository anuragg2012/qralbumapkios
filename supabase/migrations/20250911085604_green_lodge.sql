/*
  # QR Albums Database Schema

  1. New Tables
    - `users` - User accounts with email/password authentication
    - `projects` - User projects for organizing albums  
    - `project_counters` - Atomic serial number assignment per project
    - `albums` - RAW and FINAL albums with QR sharing capability
    - `album_items` - Media items (images/videos) with serial numbers
    - `viewer_sessions` - Anonymous viewer sessions for selections
    - `selections` - User selections from RAW albums

  2. Security
    - All tables have proper foreign key constraints
    - Unique constraints on critical fields (email, project keys, album slugs)
    - Serial number uniqueness enforced per project
    - Session-based selection tracking

  3. Features
    - Atomic serial number assignment using project_counters
    - Watermarked image support (separate URLs)
    - RAW->FINAL album workflow
    - Selection limits and submission tracking
*/

-- Users table
CREATE TABLE users (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  email VARCHAR(255) UNIQUE NOT NULL,
  password_hash VARCHAR(255) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Projects table  
CREATE TABLE projects (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  owner_id BIGINT NOT NULL,
  name VARCHAR(160) NOT NULL,
  `key` VARCHAR(24) UNIQUE NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (owner_id) REFERENCES users(id) ON DELETE CASCADE,
  INDEX idx_projects_owner (owner_id)
);

-- Project counters for atomic serial assignment
CREATE TABLE project_counters (
  project_id BIGINT PRIMARY KEY,
  last_serial BIGINT DEFAULT 0,
  FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE
);

-- Albums table
CREATE TABLE albums (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  project_id BIGINT NOT NULL,
  owner_id BIGINT NOT NULL,
  slug VARCHAR(16) UNIQUE NOT NULL,
  title VARCHAR(160) NOT NULL,
  version ENUM('RAW', 'FINAL') DEFAULT 'RAW',
  allow_selection BOOLEAN DEFAULT TRUE,
  selection_limit INT DEFAULT 0,
  status ENUM('ACTIVE', 'ARCHIVED') DEFAULT 'ACTIVE',
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE,
  FOREIGN KEY (owner_id) REFERENCES users(id) ON DELETE CASCADE,
  INDEX idx_albums_project (project_id),
  INDEX idx_albums_owner (owner_id),
  INDEX idx_albums_slug (slug)
);

-- Album items table
CREATE TABLE album_items (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  project_id BIGINT NOT NULL,
  album_id BIGINT NOT NULL,
  serial_no BIGINT NOT NULL,
  kind ENUM('IMAGE', 'VIDEO') NOT NULL,
  src_url TEXT NOT NULL,
  wm_url TEXT NULL,
  thumb_url TEXT NULL,
  width INT NULL,
  height INT NULL,
  bytes BIGINT NULL,
  sort_order INT DEFAULT 0,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (project_id) REFERENCES projects(id) ON DELETE CASCADE,
  FOREIGN KEY (album_id) REFERENCES albums(id) ON DELETE CASCADE,
  UNIQUE KEY uk_project_serial (project_id, serial_no),
  INDEX idx_items_album (album_id),
  INDEX idx_items_project (project_id)
);

-- Viewer sessions table
CREATE TABLE viewer_sessions (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  album_id BIGINT NOT NULL,
  session_key CHAR(22) UNIQUE NOT NULL,
  submitted BOOLEAN DEFAULT FALSE,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (album_id) REFERENCES albums(id) ON DELETE CASCADE,
  INDEX idx_sessions_album (album_id)
);

-- Selections table
CREATE TABLE selections (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  album_id BIGINT NOT NULL,
  session_key CHAR(22) NOT NULL,
  item_id BIGINT NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (album_id) REFERENCES albums(id) ON DELETE CASCADE,
  FOREIGN KEY (item_id) REFERENCES album_items(id) ON DELETE CASCADE,
  UNIQUE KEY uk_selection (album_id, session_key, item_id),
  INDEX idx_selections_album (album_id),
  INDEX idx_selections_session (session_key)
);

-- Insert sample data for development
INSERT INTO users (email, password_hash) VALUES 
('demo@earthinfosystems.com', '$2a$11$example.hash.for.password123');

INSERT INTO projects (owner_id, name, `key`) VALUES
(1, 'Wedding Photos 2024', 'WED2024ABC123');

INSERT INTO project_counters (project_id) VALUES (1);

INSERT INTO albums (project_id, owner_id, slug, title, version, selection_limit) VALUES
(1, 1, 'wed2024', 'Wedding Reception', 'RAW', 20);