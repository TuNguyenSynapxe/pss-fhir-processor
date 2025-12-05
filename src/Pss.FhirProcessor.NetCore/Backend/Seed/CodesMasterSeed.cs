namespace MOH.HealthierSG.Plugins.PSS.FhirProcessor.Api.Seed
{
    public static class CodesMasterSeed
    {
        public static string GetCodesMaster()
        {
            return @"{
  ""Questions"": [
    {
      ""QuestionCode"": ""SQ-L2H9-00000001"",
      ""QuestionDisplay"": ""Is the participant currently wearing hearing aid(s)?"",
      ""ScreeningType"": ""HS"",
      ""AllowedAnswers"": [""Yes (Proceed to next question)"", ""No (To continue with hearing screening)""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-Q1P1-00000002"",
      ""QuestionDisplay"": ""Do participant need to change your hearing aid(s)?"",
      ""ScreeningType"": ""HS"",
      ""AllowedAnswers"": [""Yes (To skip hearing screening and refer to L2 Hearing)"", ""No (To skip hearing screening and advise to continue follow up with audiologist)""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-R5W8-00000003"",
      ""QuestionDisplay"": ""Did participant proceed with Hearing Screening?"",
      ""ScreeningType"": ""HS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-P4Y1-00000004"",
      ""QuestionDisplay"": ""Visual Ear Examination (Left Ear)"",
      ""ScreeningType"": ""HS"",
      ""AllowedAnswers"": [""Pass"", ""Refer""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-S7Q0-00000005"",
      ""QuestionDisplay"": ""Visual Ear Examination (Right Ear)"",
      ""ScreeningType"": ""HS"",
      ""AllowedAnswers"": [""Pass"", ""Refer""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-J3J4-00000006"",
      ""QuestionDisplay"": ""Practice Tone (500Hz at 60dB in better ear)"",
      ""ScreeningType"": ""HS"",
      ""AllowedAnswers"": [""Pass"", ""Refer""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-F7B7-00000007"",
      ""QuestionDisplay"": ""Pure Tone Screening at 25dBHL (Left)"",
      ""ScreeningType"": ""HS"",
      ""AllowedAnswers"": [""500Hz – R"", ""500Hz – NR"", ""1000Hz – R"", ""1000Hz – NR"", ""2000Hz – R"", ""2000Hz – NR"", ""4000Hz – R"", ""4000Hz – NR""],
      ""IsMultiValue"": true
    },
    {
      ""QuestionCode"": ""SQ-B7P7-00000008"",
      ""QuestionDisplay"": ""Pure Tone Screening at 25dBHL (Right)"",
      ""ScreeningType"": ""HS"",
      ""AllowedAnswers"": [""500Hz – R"", ""500Hz – NR"", ""1000Hz – R"", ""1000Hz – NR"", ""2000Hz – R"", ""2000Hz – NR"", ""4000Hz – R"", ""4000Hz - NR""],
      ""IsMultiValue"": true
    },
    {
      ""QuestionCode"": ""SQ-C1M5-00000009"",
      ""QuestionDisplay"": ""Pure Tone Screening at 40dBHL (Left)"",
      ""ScreeningType"": ""HS"",
      ""AllowedAnswers"": [""500Hz – R"", ""500Hz – NR"", ""1000Hz – R"", ""1000Hz – NR"", ""2000Hz – R"", ""2000Hz – NR"", ""4000Hz – R"", ""4000Hz - NR""],
      ""IsMultiValue"": true
    },
    {
      ""QuestionCode"": ""SQ-C2S1-00000010"",
      ""QuestionDisplay"": ""Pure Tone Screening at 40dBHL (Right)"",
      ""ScreeningType"": ""HS"",
      ""AllowedAnswers"": [""500Hz – R"", ""500Hz – NR"", ""1000Hz – R"", ""1000Hz – NR"", ""2000Hz – R"", ""2000Hz – NR"", ""4000Hz – R"", ""4000Hz - NR""],
      ""IsMultiValue"": true
    },
    {
      ""QuestionCode"": ""SQ-M8Z2-00000011"",
      ""QuestionDisplay"": ""Does participant have any upcoming appointment with ear specialist?"",
      ""ScreeningType"": ""HS"",
      ""AllowedAnswers"": [""Yes (To continue follow up with ear specialist)"", ""No (Refer to L2 Hearing Follow Up if necessary)""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-D3S4-00000012"",
      ""QuestionDisplay"": ""Does the participant require to refer to L2 Hearing?"",
      ""ScreeningType"": ""HS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-L6T4-00000013"",
      ""QuestionDisplay"": ""Does the participant agree to proceed with L2 Hearing?"",
      ""ScreeningType"": ""HS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-J1V1-00000014"",
      ""QuestionDisplay"": ""Is the participant currently wearing dentures or has dentures?"",
      ""ScreeningType"": ""OS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-X7V6-00000015"",
      ""QuestionDisplay"": ""Placement of Denture (Upper)"",
      ""ScreeningType"": ""OS"",
      ""AllowedAnswers"": [""Upper (partial)"", ""Upper (full)""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-V1K0-00000016"",
      ""QuestionDisplay"": ""Placement of Denture (Lower)"",
      ""ScreeningType"": ""OS"",
      ""AllowedAnswers"": [""Lower (partial)"", ""Lower (full)""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-D7V0-00000017"",
      ""QuestionDisplay"": ""Did participant proceed with Oral Screening?"",
      ""ScreeningType"": ""OS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-V0X0-00000018"",
      ""QuestionDisplay"": ""Do you have any dental pain on a scale of 1 to 10? (On a scale of 1-10, 1 being not painful at all and 10 being extremely painful)"",
      ""ScreeningType"": ""OS"",
      ""AllowedAnswers"": [""Yes (Pain Scale: 6 to 10)"", ""No (Pain Scale: 1 to 5)""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-J6Q3-00000019"",
      ""QuestionDisplay"": ""Within the past 12 months, have you had any trouble speaking because of problems with your teeth, mouth or dentures?"",
      ""ScreeningType"": ""OS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-M3F4-00000020"",
      ""QuestionDisplay"": ""Within the past 12 months, have you found it difficult to eat any foods because of problems with your teeth, mouth or dentures?"",
      ""ScreeningType"": ""OS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-S3K2-00000021"",
      ""QuestionDisplay"": ""OHAT Status"",
      ""ScreeningType"": ""OS"",
      ""AllowedAnswers"": [""Healthy"", ""Require Self Care"", ""Unhealthy (Require L2 Oral Follow Up)""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-X4H3-00000022"",
      ""QuestionDisplay"": ""Does participant have any upcoming appointment with any dentist?"",
      ""ScreeningType"": ""OS"",
      ""AllowedAnswers"": [""Yes (To continue follow up with dentist)"", ""No (Refer to L2 Oral Follow Up)""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-H6B2-00000023"",
      ""QuestionDisplay"": ""Does the participant require to refer to L2 Oral?"",
      ""ScreeningType"": ""OS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-Y3R0-00000024"",
      ""QuestionDisplay"": ""Does the participant agree to proceed with L2 Oral?"",
      ""ScreeningType"": ""OS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-X1V3-00000025"",
      ""QuestionDisplay"": ""Is the participant currently wearing spectacles?"",
      ""ScreeningType"": ""VS"",
      ""AllowedAnswers"": [""Yes (To carry out vision test with participant's spectacles)"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-Z6G5-00000026"",
      ""QuestionDisplay"": ""Did participant proceed with Vision Screening?"",
      ""ScreeningType"": ""VS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-R1R7-00000027"",
      ""QuestionDisplay"": ""Snellen Test (Right Eye)\n(Please test the right eye first)"",
      ""ScreeningType"": ""VS"",
      ""AllowedAnswers"": [""6/6"", ""6/9"", ""6/12"", ""6/18"", ""6/24"", ""6/36"", ""6/60"", ""Worse than 6/60""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-F6X7-00000028"",
      ""QuestionDisplay"": ""Snellen Test (Left Eye)"",
      ""ScreeningType"": ""VS"",
      ""AllowedAnswers"": [""6/6"", ""6/9"", ""6/12"", ""6/18"", ""6/24"", ""6/36"", ""6/60"", ""Worse than 6/60""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-L9J4-00000029"",
      ""QuestionDisplay"": ""Pinhole Test (Right Eye)\n(If Snellen is worse than 6/12, please continue with Pinhole and test the right eye first)"",
      ""ScreeningType"": ""VS"",
      ""AllowedAnswers"": [""6/6"", ""6/9"", ""6/12"", ""6/18"", ""6/24"", ""6/36"", ""6/60"", ""Worse than 6/60""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-Z2W8-00000030"",
      ""QuestionDisplay"": ""Pinhole Test (Left Eye)"",
      ""ScreeningType"": ""VS"",
      ""AllowedAnswers"": [""6/6"", ""6/9"", ""6/12"", ""6/18"", ""6/24"", ""6/36"", ""6/60"", ""Worse than 6/60""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-K8L7-00000031"",
      ""QuestionDisplay"": ""Does participant have any upcoming appointment with any eye specialist or eye review at polyclinic or CHAS GP clinics?"",
      ""ScreeningType"": ""VS"",
      ""AllowedAnswers"": [""Yes (To continue follow up with the upcoming eye review in primary care or specialist clinic)"", ""No (Refer to L2 Vision Follow Up)""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-G0W6-00000032"",
      ""QuestionDisplay"": ""Does the participant require new spectacles?"",
      ""ScreeningType"": ""VS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-H4N6-00000033"",
      ""QuestionDisplay"": ""Is the participant eligible for SMF (Spectacles)?"",
      ""ScreeningType"": ""VS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-C4C4-00000034"",
      ""QuestionDisplay"": ""Has SMF (Spectacles) applied for the participant?"",
      ""ScreeningType"": ""VS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-T6W1-00000035"",
      ""QuestionDisplay"": ""Does the participant require to refer to L2 Vision?"",
      ""ScreeningType"": ""VS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    },
    {
      ""QuestionCode"": ""SQ-F1T3-00000036"",
      ""QuestionDisplay"": ""Does the participant agree to proceed with L2 Vision?"",
      ""ScreeningType"": ""VS"",
      ""AllowedAnswers"": [""Yes"", ""No""],
      ""IsMultiValue"": false
    }
  ],
  ""CodeSystems"": [
    {
      ""Id"": ""campaign-type"",
      ""Description"": ""Campaign Type"",
      ""System"": ""http://synapxe.sg/fhir/CodeSystem/campaign-type"",
      ""Concepts"": [
        { ""Code"": ""COMMUNITY"", ""Display"": ""Community"" },
        { ""Code"": ""INVITED"", ""Display"": ""Invited"" }
      ]
    },
    {
      ""Id"": ""residential-status"",
      ""Description"": ""Residential Status"",
      ""System"": ""http://synapxe.sg/fhir/CodeSystem/residential-status"",
      ""Concepts"": [
        { ""Code"": ""SINGAPORE_CITIZEN"", ""Display"": ""Singapore Citizen"" },
        { ""Code"": ""PERMANENT_RESIDENT"", ""Display"": ""Permanent Resident"" },
        { ""Code"": ""LONG_TERM_VISIT_PASS_PLUS"", ""Display"": ""Long Term Visit Pass Plus"" }
      ]
    },
    {
      ""Id"": ""subsidy"",
      ""Description"": ""Subsidy"",
      ""System"": ""http://synapxe.sg/fhir/CodeSystem/subsidy"",
      ""Concepts"": [
        { ""Code"": ""CHAS_BLUE"", ""Display"": ""CHAS Blue"" },
        { ""Code"": ""CHAS_ORANGE"", ""Display"": ""CHAS Orange"" },
        { ""Code"": ""CHAS_GREEN"", ""Display"": ""CHAS Green"" },
        { ""Code"": ""PIONEER_GENERATION"", ""Display"": ""Pioneer Generation"" },
        { ""Code"": ""MERDEKA_GENERATION"", ""Display"": ""Merdeka Generation"" }
      ]
    },
    {
      ""Id"": ""ethnicity"",
      ""Description"": ""Ethnicity"",
      ""System"": ""http://synapxe.sg/fhir/CodeSystem/ethnicity"",
      ""Concepts"": [
        { ""Code"": ""CHINESE"", ""Display"": ""Chinese"" },
        { ""Code"": ""MALAY"", ""Display"": ""Malay"" },
        { ""Code"": ""INDIAN"", ""Display"": ""Indian"" },
        { ""Code"": ""OTHERS"", ""Display"": ""Others"" }
      ]
    },
    {
      ""Id"": ""language"",
      ""Description"": ""Language"",
      ""System"": ""http://synapxe.sg/fhir/CodeSystem/language"",
      ""Concepts"": [
        { ""Code"": ""ENGLISH"", ""Display"": ""English"" },
        { ""Code"": ""MANDARIN"", ""Display"": ""Mandarin"" },
        { ""Code"": ""MALAY"", ""Display"": ""Malay"" },
        { ""Code"": ""TAMIL"", ""Display"": ""Tamil"" },
        { ""Code"": ""HOKKIEN"", ""Display"": ""Hokkien"" },
        { ""Code"": ""TEOCHEW"", ""Display"": ""Teochew"" },
        { ""Code"": ""CANTONESE"", ""Display"": ""Cantonese"" },
        { ""Code"": ""OTHERS"", ""Display"": ""Others"" }
      ]
    },
    {
      ""Id"": ""organization-type"",
      ""Description"": ""Organization Type"",
      ""System"": ""http://synapxe.sg/fhir/CodeSystem/organization-type"",
      ""Concepts"": [
        { ""Code"": ""AIC"", ""Display"": ""Agency for Integrated Care (AIC)"" },
        { ""Code"": ""SSO"", ""Display"": ""Social Service Office (SSO)"" }
      ]
    },
    {
      ""Id"": ""screening-type"",
      ""Description"": ""Screening Type"",
      ""System"": ""http://synapxe.sg/fhir/CodeSystem/screening-type"",
      ""Concepts"": [
        { ""Code"": ""HS"", ""Display"": ""Hearing Screening"" },
        { ""Code"": ""OS"", ""Display"": ""Oral Screening"" },
        { ""Code"": ""VS"", ""Display"": ""Vision Screening"" }
      ]
    }
  ]
}";
        }
    }
}
