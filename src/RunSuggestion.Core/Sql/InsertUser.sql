INSERT INTO Users (
    UserId
)
VALUES (
    @UserId
)
RETURNING Id;