# Docker Shit

        docker build -f .\CANSlaveShimAPI\Dockerfile . -t metrol/canslaveapi
        docker run --name canslaveapi --rm -p 8080:8080 metrol/canslaveapi
