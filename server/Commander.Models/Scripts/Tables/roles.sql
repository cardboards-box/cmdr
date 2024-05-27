CREATE TABLE IF NOT EXISTS cmdr_roles (
	id UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
	
	name TEXT NOT NULL UNIQUE,
	description TEXT NOT NULL,
	icon TEXT NOT NULL,
	ordinal NUMERIC NOT NULL,

	created_by UUID,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	updated_by UUID,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
	deleted_by UUID,
	deleted_at TIMESTAMP
);


WITH roles_base 
(name,			description,								icon,									ordinal) AS (VALUES
('Admin',		'User is a system administrator',			'<a:sparkleEyes:1150466426084270223>',	1),
('Moderator',	'User is special but not the specialest',	'<a:sip:1150466101323501588>',			2),
('User',		'A general user of the platform',			'<:meguuutehe:1109943802510184530>',	3),
('Agent',		'User is a queue agent account',			'<a:HuTaoMoneyRain:967965230086885446>',4))
INSERT INTO cmdr_roles (name, description, icon, ordinal)
SELECT s.name, s.description, s.icon, s.ordinal
FROM roles_base s
LEFT JOIN cmdr_roles t ON s.name = t.name
WHERE t.id IS NULL;
