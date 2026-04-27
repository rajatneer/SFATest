export function errorHandler(error, req, res, next) {
  console.error("Unhandled API error", {
    message: error?.message,
    stack: error?.stack,
    path: req?.originalUrl,
    method: req?.method
  });

  if (res.headersSent) {
    return next(error);
  }

  return res.status(500).json({
    error: "Unexpected server error."
  });
}