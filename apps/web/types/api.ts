/**
 * Shared shapes for the API layer. Feature-specific response/request
 * types should live in `types/<feature>.ts` and extend these, not
 * duplicate them.
 */

export interface ApiSuccess<T> {
  success: true;
  data: T;
  meta?: Record<string, unknown>;
}

export interface ApiFailure {
  success: false;
  error: {
    code: string;
    message: string;
    fields?: Record<string, string>;
  };
}

export type ApiResult<T> = ApiSuccess<T> | ApiFailure;

export class ApiError extends Error {
  code: string;
  status: number;
  fields?: Record<string, string>;

  constructor(params: {
    code: string;
    message: string;
    status: number;
    fields?: Record<string, string>;
  }) {
    super(params.message);
    this.name = "ApiError";
    this.code = params.code;
    this.status = params.status;
    this.fields = params.fields;
  }
}
