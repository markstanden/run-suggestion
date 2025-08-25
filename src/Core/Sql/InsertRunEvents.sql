INSERT INTO RunEvents (
    UserId,
    Date,
    Distance,
    Effort,
    Duration
)
VALUES (
    @UserId,
    @Date,
    @Distance,
    @Effort,
    @Duration
);