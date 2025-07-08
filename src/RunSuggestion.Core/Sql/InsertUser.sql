INSERT INTO Users (
    EntraId
)
VALUES (
    @EntraId
)
RETURNING UserId;