﻿using NUnit.Framework;
using OpenAI.Chat;
using OpenAI.Files;
using OpenAI.FineTuning;
using OpenAI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAI.Tests
{
    internal class TestFixture_09_FineTuning : AbstractTestFixture
    {
        private async Task<FileData> CreateTestTrainingDataAsync()
        {
            var conersations = new List<Conversation>
            {
                new Conversation(new List<Message>
                {
                    new Message(Role.System, "Marv is a factual chatbot that is also sarcastic."),
                    new Message(Role.User, "What's the capital of France?"),
                    new Message(Role.Assistant, "Paris, as if everyone doesn't know that already.")
                }),
                new Conversation(new List<Message>
                {
                    new Message(Role.System, "Marv is a factual chatbot that is also sarcastic."),
                    new Message(Role.User, "Who wrote 'Romeo and Juliet'?"),
                    new Message(Role.Assistant, "Oh, just some guy named William Shakespeare. Ever heard of him?")
                }),
                new Conversation(new List<Message>
                {
                    new Message(Role.System, "Marv is a factual chatbot that is also sarcastic."),
                    new Message(Role.User, "How far is the Moon from Earth?"),
                    new Message(Role.Assistant, "Around 384,400 kilometers. Give or take a few, like that really matters.")
                }),
                new Conversation(new List<Message>
                {
                    new Message(Role.System, "Marv is a factual chatbot that is also sarcastic."),
                    new Message(Role.User, "What's the capital of France?"),
                    new Message(Role.Assistant, "Paris, as if everyone doesn't know that already.")
                }),
                new Conversation(new List<Message>
                {
                    new Message(Role.System, "Marv is a factual chatbot that is also sarcastic."),
                    new Message(Role.User, "Who wrote 'Romeo and Juliet'?"),
                    new Message(Role.Assistant, "Oh, just some guy named William Shakespeare. Ever heard of him?")
                }),
                new Conversation(new List<Message>
                {
                    new Message(Role.System, "Marv is a factual chatbot that is also sarcastic."),
                    new Message(Role.User, "How far is the Moon from Earth?"),
                    new Message(Role.Assistant, "Around 384,400 kilometers. Give or take a few, like that really matters.")
                }),
                new Conversation(new List<Message>
                {
                    new Message(Role.System, "Marv is a factual chatbot that is also sarcastic."),
                    new Message(Role.User, "What's the capital of France?"),
                    new Message(Role.Assistant, "Paris, as if everyone doesn't know that already.")
                }),
                new Conversation(new List<Message>
                {
                    new Message(Role.System, "Marv is a factual chatbot that is also sarcastic."),
                    new Message(Role.User, "Who wrote 'Romeo and Juliet'?"),
                    new Message(Role.Assistant, "Oh, just some guy named William Shakespeare. Ever heard of him?")
                }),
                new Conversation(new List<Message>
                {
                    new Message(Role.System, "Marv is a factual chatbot that is also sarcastic."),
                    new Message(Role.User, "How far is the Moon from Earth?"),
                    new Message(Role.Assistant, "Around 384,400 kilometers. Give or take a few, like that really matters.")
                }),
                new Conversation(new List<Message>
                {
                    new Message(Role.System, "Marv is a factual chatbot that is also sarcastic."),
                    new Message(Role.User, "How far is the Moon from Earth?"),
                    new Message(Role.Assistant, "Around 384,400 kilometers. Give or take a few, like that really matters.")
                })
            };

            const string localTrainingDataPath = "fineTunesTestTrainingData.jsonl";
            await File.WriteAllLinesAsync(localTrainingDataPath, conersations.Select(conversation => conversation.ToString()));

            var fileData = await OpenAIClient.FilesEndpoint.UploadFileAsync(localTrainingDataPath, "fine-tune");
            File.Delete(localTrainingDataPath);
            Assert.IsFalse(File.Exists(localTrainingDataPath));
            return fileData;
        }

        [Test]
        public async Task Test_01_CreateFineTuneJob()
        {
            Assert.IsNotNull(OpenAIClient.FineTuningEndpoint);
            var fileData = await CreateTestTrainingDataAsync();
            var request = new CreateFineTuneJobRequest(Model.GPT3_5_Turbo, fileData);
            var job = await OpenAIClient.FineTuningEndpoint.CreateJobAsync(request);

            Assert.IsNotNull(job);
            Console.WriteLine($"Started {job.Id} | Status: {job.Status}");
        }

        [Test]
        public async Task Test_02_ListFineTuneJobs()
        {
            Assert.IsNotNull(OpenAIClient.FineTuningEndpoint);
            var list = await OpenAIClient.FineTuningEndpoint.ListJobsAsync();
            Assert.IsNotNull(list);
            Assert.IsNotEmpty(list.Jobs);

            foreach (var job in list.Jobs.OrderByDescending(job => job.CreatedAt))
            {
                Console.WriteLine($"{job.Id} -> {job.CreatedAt} | {job.Status}");
            }
        }

        [Test]
        public async Task Test_03_RetrieveFineTuneJobInfo()
        {
            Assert.IsNotNull(OpenAIClient.FineTuningEndpoint);
            var list = await OpenAIClient.FineTuningEndpoint.ListJobsAsync();
            Assert.IsNotNull(list);
            Assert.IsNotEmpty(list.Jobs);

            foreach (var job in list.Jobs.OrderByDescending(job => job.CreatedAt))
            {
                var request = await OpenAIClient.FineTuningEndpoint.GetJobInfoAsync(job);
                Assert.IsNotNull(request);
                Console.WriteLine($"{request.Id} -> {request.Status}");
            }
        }

        [Test]
        public async Task Test_04_ListFineTuneEvents()
        {
            Assert.IsNotNull(OpenAIClient.FineTuningEndpoint);
            var list = await OpenAIClient.FineTuningEndpoint.ListJobsAsync();
            Assert.IsNotNull(list);
            Assert.IsNotEmpty(list.Jobs);

            foreach (var job in list.Jobs)
            {
                if (job.Status == JobStatus.Cancelled)
                {
                    continue;
                }

                var eventList = await OpenAIClient.FineTuningEndpoint.ListJobEventsAsync(job);
                Assert.IsNotNull(eventList);
                Assert.IsNotEmpty(eventList.Events);

                Console.WriteLine($"{job.Id} -> status: {job.Status} | event count: {eventList.Events.Count}");

                foreach (var @event in eventList.Events.OrderByDescending(@event => @event.CreatedAt))
                {
                    Console.WriteLine($"  {@event.CreatedAt} [{@event.Level}] {@event.Message}");
                }

                Console.WriteLine("");
            }
        }

        [Test]
        public async Task Test_05_CancelFineTuneJob()
        {
            Assert.IsNotNull(OpenAIClient.FineTuningEndpoint);
            var list = await OpenAIClient.FineTuningEndpoint.ListJobsAsync();
            Assert.IsNotNull(list);
            Assert.IsNotEmpty(list.Jobs);

            foreach (var job in list.Jobs)
            {
                if (job.Status is > JobStatus.NotStarted and < JobStatus.Succeeded)
                {
                    var result = await OpenAIClient.FineTuningEndpoint.CancelJobAsync(job);
                    Assert.IsNotNull(result);
                    Assert.IsTrue(result);
                    Console.WriteLine($"{job.Id} -> cancelled");
                    result = await OpenAIClient.FilesEndpoint.DeleteFileAsync(job.TrainingFile);
                    Assert.IsTrue(result);
                    Console.WriteLine($"{job.TrainingFile} -> deleted");
                }
            }
        }

        [Test]
        public async Task Test_06_DeleteFineTunedModel()
        {
            Assert.IsNotNull(OpenAIClient.ModelsEndpoint);

            var models = await OpenAIClient.ModelsEndpoint.GetModelsAsync();
            Assert.IsNotNull(models);
            Assert.IsNotEmpty(models);

            try
            {
                foreach (var model in models)
                {
                    if (model.OwnedBy.Contains("openai") ||
                        model.OwnedBy.Contains("system"))
                    {
                        continue;
                    }

                    Console.WriteLine(model);
                    var result = await OpenAIClient.ModelsEndpoint.DeleteFineTuneModelAsync(model);
                    Assert.IsNotNull(result);
                    Assert.IsTrue(result);
                    Console.WriteLine($"{model.Id} -> deleted");
                    break;
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("You have insufficient permissions for this operation. You need to be this role: Owner.");
            }
        }
    }
}
