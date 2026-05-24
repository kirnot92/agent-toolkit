# AgentToolkit

AgentToolkit is a lightweight .NET library for building tool-calling workflows around LLM chat clients.

The project provides shared message and tool definitions, attribute-based tool metadata, a tool manager for parsing and invoking registered tools, and client adapters for OpenAI-compatible chat completion flows.

## Project Layout

- `AgentToolkit`: core library.
- `AgentToolkit.Samples`: local sample project for exercising the library.
- `AgentToolkit.Tests`: unit tests for message handling and tool management.

## Scope

AgentToolkit focuses on the small pieces needed to connect application-defined tools with chat-based LLM clients:

- define tools and arguments with attributes;
- convert registered tools into model-facing definitions;
- parse and execute model-requested tool calls;
- represent user, assistant, and tool messages consistently;
- call OpenAI and llama.cpp-compatible chat clients through a common interface.

