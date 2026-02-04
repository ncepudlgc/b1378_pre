# Development environment for .NET 9.0 application
FROM mcr.microsoft.com/dotnet/sdk:9.0

# Set working directory
WORKDIR /app

# Copy project files
COPY . .

# The container will provide a shell for development
# No build or run commands here - the app may have intentional bugs
CMD ["/bin/bash"]