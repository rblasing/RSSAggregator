CREATE TABLE state
(
   name NVARCHAR(25) NOT NULL,
   abbr NCHAR(2) NOT NULL,
   ref_count INT NOT NULL DEFAULT 0
);

INSERT INTO state (name, abbr) VALUES ('ALABAMA', 'AL');
INSERT INTO state (name, abbr) VALUES ('ALASKA', 'AK');
INSERT INTO state (name, abbr) VALUES ('ARIZONA', 'AZ');
INSERT INTO state (name, abbr) VALUES ('ARKANSAS', 'AR');
INSERT INTO state (name, abbr) VALUES ('CALIFORNIA', 'CA');
INSERT INTO state (name, abbr) VALUES ('COLORADO', 'CO');
INSERT INTO state (name, abbr) VALUES ('CONNECTICUT', 'CT');
INSERT INTO state (name, abbr) VALUES ('DISTRICT OF COLUMBIA', 'DC');
INSERT INTO state (name, abbr) VALUES ('DELAWARE', 'DE');
INSERT INTO state (name, abbr) VALUES ('FLORIDA', 'FL');
INSERT INTO state (name, abbr) VALUES ('GEORGIA', 'GA');
INSERT INTO state (name, abbr) VALUES ('HAWAII', 'HI');
INSERT INTO state (name, abbr) VALUES ('IDAHO', 'ID');
INSERT INTO state (name, abbr) VALUES ('ILLINOIS', 'IL');
INSERT INTO state (name, abbr) VALUES ('INDIANA', 'IN');
INSERT INTO state (name, abbr) VALUES ('IOWA', 'IA');
INSERT INTO state (name, abbr) VALUES ('KANSAS', 'KS');
INSERT INTO state (name, abbr) VALUES ('KENTUCKY', 'KY');
INSERT INTO state (name, abbr) VALUES ('LOUISIANA', 'LA');
INSERT INTO state (name, abbr) VALUES ('MAINE', 'ME');
INSERT INTO state (name, abbr) VALUES ('MARYLAND', 'MD');
INSERT INTO state (name, abbr) VALUES ('MASSACHUSETTS', 'MA');
INSERT INTO state (name, abbr) VALUES ('MICHIGAN', 'MI');
INSERT INTO state (name, abbr) VALUES ('MINNESOTA', 'MN');
INSERT INTO state (name, abbr) VALUES ('MISSISSIPPI', 'MS');
INSERT INTO state (name, abbr) VALUES ('MISSOURI', 'MO');
INSERT INTO state (name, abbr) VALUES ('MONTANA', 'MT');
INSERT INTO state (name, abbr) VALUES ('NEBRASKA', 'NE');
INSERT INTO state (name, abbr) VALUES ('NEVADA', 'NV');
INSERT INTO state (name, abbr) VALUES ('NEW HAMPSHIRE', 'NH');
INSERT INTO state (name, abbr) VALUES ('NEW JERSEY', 'NJ');
INSERT INTO state (name, abbr) VALUES ('NEW MEXICO', 'NM');
INSERT INTO state (name, abbr) VALUES ('NEW YORK', 'NY');
INSERT INTO state (name, abbr) VALUES ('NORTH CAROLINA', 'NC');
INSERT INTO state (name, abbr) VALUES ('N CAROLINA', 'NC');
INSERT INTO state (name, abbr) VALUES ('N. CAROLINA', 'NC');
INSERT INTO state (name, abbr) VALUES ('NORTH DAKOTA', 'ND');
INSERT INTO state (name, abbr) VALUES ('N DAKOTA', 'ND');
INSERT INTO state (name, abbr) VALUES ('N. DAKOTA', 'ND');
INSERT INTO state (name, abbr) VALUES ('OHIO', 'OH');
INSERT INTO state (name, abbr) VALUES ('OKLAHOMA', 'OK');
INSERT INTO state (name, abbr) VALUES ('OREGON', 'OR');
INSERT INTO state (name, abbr) VALUES ('PENNSYLVANIA', 'PA');
INSERT INTO state (name, abbr) VALUES ('RHODE ISLAND', 'RI');
INSERT INTO state (name, abbr) VALUES ('SOUTH CAROLINA', 'SC');
INSERT INTO state (name, abbr) VALUES ('S CAROLINA', 'SC');
INSERT INTO state (name, abbr) VALUES ('S. CAROLINA', 'SC');
INSERT INTO state (name, abbr) VALUES ('SOUTH DAKOTA', 'SD');
INSERT INTO state (name, abbr) VALUES ('S DAKOTA', 'SD');
INSERT INTO state (name, abbr) VALUES ('S. DAKOTA', 'SD');
INSERT INTO state (name, abbr) VALUES ('TENNESSEE', 'TN');
INSERT INTO state (name, abbr) VALUES ('TEXAS', 'TX');
INSERT INTO state (name, abbr) VALUES ('UTAH', 'UT');
INSERT INTO state (name, abbr) VALUES ('VERMONT', 'VT');
INSERT INTO state (name, abbr) VALUES ('VIRGINIA', 'VA');
INSERT INTO state (name, abbr) VALUES ('WASHINGTON', 'WA');
INSERT INTO state (name, abbr) VALUES ('WEST VIRGINIA', 'WV');
INSERT INTO state (name, abbr) VALUES ('W VIRGINIA', 'WV');
INSERT INTO state (name, abbr) VALUES ('W. VIRGINIA', 'WV');
INSERT INTO state (name, abbr) VALUES ('WASHINGTON, D.C.', 'DC');
INSERT INTO state (name, abbr) VALUES ('WASHINGTON D.C.', 'DC');
INSERT INTO state (name, abbr) VALUES ('WASHINGTON, DC', 'DC');
INSERT INTO state (name, abbr) VALUES ('WASHINGTON DC', 'DC');
INSERT INTO state (name, abbr) VALUES ('WISCONSIN', 'WI');
INSERT INTO state (name, abbr) VALUES ('WYOMING', 'WY');




SELECT s.abbr, COUNT(*) FROM state s, rss_item i WHERE i.title LIKE '%' + s.name + '%' OR xml.exist('/item/description/text()[contains(lower-case(.), sql:column("s.name"))]') = 1 GROUP BY s.abbr;
UPDATE state SET ref_count = (SELECT COUNT(*) FROM rss_item WHERE 
   (rss_item.title LIKE '%' + state.name + '%' OR 
   rss_item.xml.exist('/item/description/text()[contains(lower-case(.), sql:column("state.name"))]') = 1) AND 
   (SELECT rss_feed.regional FROM rss_feed WHERE rss_item.feed_id = rss_feed.feed_id) = 0);


SELECT s.abbr, COUNT(*) FROM state s, rss_item i WHERE i.title LIKE '%' + s.name + '%' GROUP BY s.abbr;

SELECT s.abbr, COUNT(*), SUM(CASE WHEN i.ins_date > '2020-11-23' THEN 1 ELSE 0 END) FROM state s, rss_item i WHERE 
i.title LIKE '%' + s.name + '%' GROUP BY s.abbr;