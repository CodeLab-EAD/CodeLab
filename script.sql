CREATE DATABASE dbCodeLab;
USE dbCodeLab;

CREATE TABLE tb_usuario(
	id_usuario INT PRIMARY KEY AUTO_INCREMENT,
	nome_usuario VARCHAR(255) NOT NULL,
	email VARCHAR(255) NOT NULL,
	nivelAcesso VARCHAR(255) NOT NULL,
	senha VARCHAR(255) NOT NULL
);

CREATE TABLE tb_curso(
	id_curso INT PRIMARY KEY AUTO_INCREMENT,
	duracao VARCHAR(255) NOT NULL,
	nome_curso VARCHAR(255) NOT NULL,
    foto_curso VARCHAR(255)
);

CREATE TABLE tb_instrutor(
	id_instrutor INT PRIMARY KEY AUTO_INCREMENT,
	nome_instrutor VARCHAR(255) NOT NULL,
	formacao VARCHAR(255) NOT NULL
);

CREATE TABLE tb_aula(
	id_aula INT PRIMARY KEY AUTO_INCREMENT,
	id_instrutor INT NOT NULL,
	id_curso INT NOT NULL
);

ALTER TABLE tb_aula
	ADD CONSTRAINT fk_aula_instrutor FOREIGN KEY (id_instrutor)
		REFERENCES tb_instrutor(id_instrutor) ON DELETE CASCADE ON UPDATE CASCADE,
	ADD CONSTRAINT fk_aula_curso FOREIGN KEY (id_curso)
		REFERENCES tb_curso(id_curso) ON DELETE CASCADE ON UPDATE CASCADE;

DELIMITER $$

-- CADASTRAR USUÁRIO
DROP PROCEDURE IF EXISTS sp_cadastrar_usuario $$
CREATE PROCEDURE sp_cadastrar_usuario(
	IN p_nome VARCHAR(255),
	IN p_email VARCHAR(255),
	IN p_role VARCHAR(255),
	IN p_senha VARCHAR(255)
)
BEGIN
	INSERT INTO tb_usuario (nome_usuario, email, nivelAcesso, senha)
	VALUES (p_nome, p_email, p_role, p_senha);
END $$

-- CADASTRAR CURSO
DROP PROCEDURE IF EXISTS sp_cadastrar_curso $$
CREATE PROCEDURE sp_cadastrar_curso(
	IN p_duracao VARCHAR(255),
	IN p_nome_curso VARCHAR(255),
	IN p_foto_curso VARCHAR(255)
)
BEGIN
	INSERT INTO tb_curso (duracao, nome_curso, foto_curso)
	VALUES (p_duracao, p_nome_curso, p_foto_curso);
END $$

-- CADASTRAR INSTRUTOR
DROP PROCEDURE IF EXISTS sp_cadastrar_instrutor $$
CREATE PROCEDURE sp_cadastrar_instrutor(
	IN p_nome_instrutor VARCHAR(255),
	IN p_formacao VARCHAR(255)
)
BEGIN
	INSERT INTO tb_instrutor (nome_instrutor, formacao)
	VALUES (p_nome_instrutor, p_formacao);
END $$

-- CADASTRAR AULA
DROP PROCEDURE IF EXISTS sp_cadastrar_aula $$
CREATE PROCEDURE sp_cadastrar_aula(
	IN p_id_instrutor INT,
	IN p_id_curso INT
)
BEGIN
	INSERT INTO tb_aula (id_instrutor, id_curso)
	VALUES (p_id_instrutor, p_id_curso);
END $$

DROP PROCEDURE IF EXISTS sp_listar_usuario $$
CREATE PROCEDURE sp_listar_usuario()
BEGIN
	SELECT id_usuario, nome_usuario, email, nivelAcesso
	FROM tb_usuario
	ORDER BY nome_usuario;
END $$

DROP PROCEDURE IF EXISTS sp_listar_curso $$
CREATE PROCEDURE sp_listar_curso()
BEGIN
	SELECT id_curso, nome_curso, duracao, foto_curso
	FROM tb_curso
	ORDER BY nome_curso;
END $$

DROP PROCEDURE IF EXISTS sp_listar_instrutor $$
CREATE PROCEDURE sp_listar_instrutor()
BEGIN
	SELECT id_instrutor, nome_instrutor, formacao
	FROM tb_instrutor
	ORDER BY nome_instrutor;
END $$

DROP PROCEDURE IF EXISTS sp_listar_aula $$
CREATE PROCEDURE sp_listar_aula()
BEGIN
	SELECT 
		a.id_aula,
		i.nome_instrutor AS instrutor,
		i.formacao,
		c.nome_curso AS curso,
		c.duracao
	FROM tb_aula a
	INNER JOIN tb_instrutor i ON i.id_instrutor = a.id_instrutor
	INNER JOIN tb_curso c ON c.id_curso = a.id_curso
	ORDER BY a.id_aula;
END $$

DROP PROCEDURE IF EXISTS sp_atualizar_usuario $$
CREATE PROCEDURE sp_atualizar_usuario(
	IN p_id_usuario INT,
	IN p_nome VARCHAR(255),
	IN p_email VARCHAR(255),
	IN p_role VARCHAR(255),
	IN p_senha VARCHAR(255)
)
BEGIN
	UPDATE tb_usuario
	SET nome_usuario = IFNULL(p_nome, nome_usuario),
		email = IFNULL(p_email, email),
		nivelAcesso = IFNULL(p_role, nivelAcesso),
		senha = IFNULL(p_senha, senha)
	WHERE id_usuario = p_id_usuario;
END $$

DROP PROCEDURE IF EXISTS sp_atualizar_curso $$
CREATE PROCEDURE sp_atualizar_curso(
	IN p_id_curso INT,
	IN p_duracao VARCHAR(255),
	IN p_nome_curso VARCHAR(255),
	IN p_foto_curso VARCHAR(255)
)
BEGIN
	UPDATE tb_curso
	SET duracao = IFNULL(p_duracao, duracao),
		nome_curso = IFNULL(p_nome_curso, nome_curso),
		foto_curso = IFNULL(p_foto_curso, foto_curso)
	WHERE id_curso = p_id_curso;
END $$

DROP PROCEDURE IF EXISTS sp_atualizar_instrutor $$
CREATE PROCEDURE sp_atualizar_instrutor(
	IN p_id_instrutor INT,
	IN p_nome_instrutor VARCHAR(255),
	IN p_formacao VARCHAR(255)
)
BEGIN
	UPDATE tb_instrutor
	SET nome_instrutor = IFNULL(p_nome_instrutor, nome_instrutor),
		formacao = IFNULL(p_formacao, formacao)
	WHERE id_instrutor = p_id_instrutor;
END $$

DROP PROCEDURE IF EXISTS sp_atualizar_aula $$
CREATE PROCEDURE sp_atualizar_aula(
	IN p_id_aula INT,
	IN p_id_instrutor INT,
	IN p_id_curso INT
)
BEGIN
	UPDATE tb_aula
	SET id_instrutor = IFNULL(p_id_instrutor, id_instrutor),
		id_curso = IFNULL(p_id_curso, id_curso)
	WHERE id_aula = p_id_aula;
END $$

DROP PROCEDURE IF EXISTS sp_excluir_usuario $$
CREATE PROCEDURE sp_excluir_usuario(IN p_id_usuario INT)
BEGIN
	DELETE FROM tb_usuario WHERE id_usuario = p_id_usuario;
END $$

DROP PROCEDURE IF EXISTS sp_excluir_curso $$
CREATE PROCEDURE sp_excluir_curso(IN p_id_curso INT)
BEGIN
	DELETE FROM tb_curso WHERE id_curso = p_id_curso;
END $$

DROP PROCEDURE IF EXISTS sp_excluir_instrutor $$
CREATE PROCEDURE sp_excluir_instrutor(IN p_id_instrutor INT)
BEGIN
	DELETE FROM tb_instrutor WHERE id_instrutor = p_id_instrutor;
END $$

DROP PROCEDURE IF EXISTS sp_excluir_aula $$
CREATE PROCEDURE sp_excluir_aula(IN p_id_aula INT)
BEGIN
	DELETE FROM tb_aula WHERE id_aula = p_id_aula;
END $$

DROP PROCEDURE IF EXISTS sp_usuario_obter $$
CREATE PROCEDURE sp_usuario_obter(IN p_id_usuario INT)
BEGIN
	SELECT * FROM tb_usuario WHERE id_usuario = p_id_usuario;
END $$

DROP PROCEDURE IF EXISTS sp_usuario_obter_email $$
CREATE PROCEDURE sp_usuario_obter_email(IN p_email_usuario VARCHAR(255))
BEGIN
	SELECT id_usuario, email, senha, nome_usuario, nivelAcesso
	FROM tb_usuario
	WHERE email = p_email_usuario;
END $$

DROP PROCEDURE IF EXISTS sp_curso_obter $$
CREATE PROCEDURE sp_curso_obter(IN p_id_curso INT)
BEGIN
	SELECT * FROM tb_curso WHERE id_curso = p_id_curso;
END $$

DROP PROCEDURE IF EXISTS sp_instrutor_obter $$
CREATE PROCEDURE sp_instrutor_obter(IN p_id_instrutor INT)
BEGIN
	SELECT id_instrutor, nome_instrutor, formacao FROM tb_instrutor WHERE id_instrutor = p_id_instrutor;
END $$

DROP PROCEDURE IF EXISTS sp_aula_obter $$
CREATE PROCEDURE sp_aula_obter(IN p_id_aula INT)
BEGIN
	SELECT 
		a.id_aula,
		i.nome_instrutor,
		i.formacao,
		c.nome_curso,
		c.duracao
	FROM tb_aula a
	INNER JOIN tb_instrutor i ON i.id_instrutor = a.id_instrutor
	INNER JOIN tb_curso c ON c.id_curso = a.id_curso
	WHERE a.id_aula = p_id_aula;
END $$

DELIMITER ;
SELECT * FROM tb_usuario;
SELECT * FROM tb_curso;
