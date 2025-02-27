# Payfast Integration API

## Overview

This repository provides a .NET 9.0 API integration for Payfast, a South African payment gateway. The integration enables secure payment processing, including payment URL generation and Instant Payment Notification (IPN) validation.

## Features

- Generate Payfast payment request URLs.
- Validate Instant Payment Notifications (IPN).
- API versioning and OpenAPI documentation.
- Scalar UI available at `https://localhost:7099/scalar/v1` for enhanced API interaction.
- Configurable environment variables for secure deployment.

## Installation

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or later

### Clone Repository

```sh
git clone https://github.com/Pierre35557/payfast-dotnet.git
cd payfast-dotnet
```

### Set Up Environment Variables

Create a `dev.env` file in the `Payfast.API` project root and populate it with your Payfast credentials:

```env
PAYFAST_MERCHANT_ID=
PAYFAST_MERCHANT_KEY=
PAYFAST_PASSPHRASE=
PAYFAST_URL=
PAYFAST_SANDBOX_URL=
PAYFAST_RETURN_URL=
PAYFAST_NOTIFY_URL=
PAYFAST_CANCEL_URL=
USE_PAYFAST_SANDBOX=
```

## Running the Application

### Local Development

```sh
dotnet run
```

The API will be accessible at `https://localhost:7009` (or another port if configured differently).

## API Endpoints

### Generate Payment URL

**Endpoint:** `POST /api/v1/payfast/generate-payment-url`

**Request Body:**

```json
{
  "name": "John",
  "surname": "Doe",
  "email": "john.doe@example.com",
  "mobileNumber": "0000000000",
  "itemName": "Test Product",
  "amount": 100,
  "confirmEmail": true
}
```

**Response:**

```json
{
  "success": true,
  "message": "Payment URL generated",
  "data": {
    "paymentUrl": "https://sandbox.payfast.co.za/eng/process?amount=100..."
  },
  "errors": null,
  "statusCode": 201
}
```

### Bad Validation Example

**Endpoint:** `POST /api/v1/payfast/generate-payment-url`

**Request Body:**

```json
{
  "name": null,
  "surname": "Doe",
  "email": "john.doe@example.com",
  "mobileNumber": "0000000000",
  "itemName": null,
  "amount": 100,
  "confirmEmail": true
}
```

**Response:**

```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "The Name field is required.",
    "The ItemName field is required."
  ],
  "statusCode": 400
}
```

### Validate Payfast IPN

**Endpoint:** `POST /api/v1/payfast/validate-ipn`
**Request Body:** Form data received from Payfast IPN.

## OpenAPI & Scalar UI

The API documentation is available via Scalar UI at:

```
https://localhost:7099/scalar/v1
```

This provides an interactive UI for testing and exploring the API.

## Contributing

1. Fork the repository.
2. Create a new branch (`feature-branch`).
3. Commit your changes.
4. Push to the branch.
5. Open a pull request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

