# Vendor Managment API

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
- [Know Issues](#know-issues)

## Installation

Installed Docker with Compose is requred.

```shell
git clone https://github.com/iBubelo/VendorAPI.git
cd VendorAPI
docker compose up --build
```

## Usage

Project configered to run using Development environment with Swagger by default.

Open <http://localhost:8080/swagger/index.html> in you browser.

JWT Bearer token Auth and Role based authorization were implemented.
Token should be obtanade with one of the users below.
Database is initilised with two roles and some data.

"Admin", has all rights. Can manage users.

```json
{
    "email": "admin@example.com",
    "password": "Admin123!"
}
```

"Manager", can create and manage Vendor, Bank Account and Contact Person, but not delete.

```json
{
    "email": "manager@example.com",
    "password": "Manager123!"
}
```

## Know Issues

- Test are missing.
- Input validation should be improved.
- Error handling is mininal.
- Rasid cache can be improved.
- Documentation is missing.
- Security (i.e. rate limiting) should be improved.
