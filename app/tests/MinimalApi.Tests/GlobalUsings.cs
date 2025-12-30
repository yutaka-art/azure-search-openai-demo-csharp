// Copyright (c) Microsoft. All rights reserved.

global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using Azure;
global using Azure.AI.OpenAI;
global using Azure.Identity;
global using Azure.Search.Documents;
global using Azure.Search.Documents.Models;
global using FluentAssertions;
global using Microsoft.Extensions.Configuration;
global using MinimalApi.Services;
global using NSubstitute;
global using OpenAI;
global using OpenAI.Embeddings;
global using Shared.Models;
global using Xunit;

