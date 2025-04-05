variable "project_name" {
  description = "Base name for all resources"
  type        = string
  default     = "runsuggestion"
}

variable "environment" {
  description = "Environment name (dev, prod)"
  type        = string
  default     = "dev"
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "westeurope"
}