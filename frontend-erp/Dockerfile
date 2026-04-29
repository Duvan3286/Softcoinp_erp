# Build stage
FROM node:20-slim AS build
WORKDIR /app

# Install dependencies
COPY package*.json ./
RUN npm install

# Copy source and build
COPY . .
RUN npm run build

# Runtime stage
FROM node:20-slim AS final
WORKDIR /app
COPY --from=build /app/package*.json ./
COPY --from=build /app/.next ./.next
COPY --from=build /app/public ./public
COPY --from=build /app/node_modules ./node_modules

EXPOSE 3000
CMD ["npm", "start"]
