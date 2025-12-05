/**
 * Generate sample FHIR bundle based on validation metadata
 */

export const generateSampleBundle = (metadata) => {
  if (!metadata || !metadata.CodesMaster) {
    return getMinimalSampleBundle();
  }

  const bundle = {
    resourceType: "Bundle",
    type: "collection",
    entry: []
  };

  // Add Patient
  bundle.entry.push({
    resource: generatePatientResource()
  });

  // Add Encounter
  bundle.entry.push({
    resource: generateEncounterResource()
  });

  // Add HealthcareService
  bundle.entry.push({
    resource: generateHealthcareServiceResource()
  });

  // Add Location
  bundle.entry.push({
    resource: generateLocationResource()
  });

  // Add Provider Organization
  bundle.entry.push({
    resource: generateOrganizationResource("provider")
  });

  // Add Cluster Organization
  bundle.entry.push({
    resource: generateOrganizationResource("cluster")
  });

  // Add Observation resources based on CodesMaster questions
  const screeningTypes = ["HS", "OS", "VS"];
  screeningTypes.forEach(screeningType => {
    const questions = metadata.CodesMaster.Questions.filter(
      q => q.ScreeningType === screeningType
    );
    
    if (questions.length > 0) {
      bundle.entry.push({
        resource: generateObservationResource(screeningType, questions, metadata)
      });
    }
  });

  return bundle;
};

const generatePatientResource = () => ({
  resourceType: "Patient",
  id: "patient-001",
  identifier: [
    {
      system: "https://fhir.synapxe.sg/NamingSystem/nric",
      value: "S9988776C"
    }
  ],
  name: [{ text: "Chandra Rao" }],
  gender: "male",
  birthDate: "1955-05-20",
  telecom: [
    {
      system: "phone",
      value: "98765432",
      use: "mobile"
    }
  ],
  address: [
    {
      line: ["789 Bedok South Ave 2 #02-11"],
      postalCode: "460789"
    }
  ],
  communication: [
    {
      language: {
        coding: [
          {
            system: "urn:ietf:bcp:47",
            code: "ms",
            display: "Malay"
          }
        ]
      },
      preferred: true
    }
  ],
  maritalStatus: {
    coding: [
      {
        system: "http://terminology.hl7.org/CodeSystem/v3-MaritalStatus",
        code: "M",
        display: "Married"
      }
    ]
  },
  contact: [
    {
      relationship: [
        {
          coding: [
            {
              system: "http://terminology.hl7.org/CodeSystem/v2-0131",
              code: "C",
              display: "Emergency Contact"
            }
          ]
        }
      ],
      name: { text: "Jane Rao" },
      telecom: [
        {
          system: "phone",
          value: "91234567"
        }
      ]
    }
  ],
  extension: [
    {
      url: "https://fhir.synapxe.sg/StructureDefinition/ext-ethnicity",
      valueCodeableConcept: {
        coding: [
          {
            system: "https://fhir.synapxe.sg/CodeSystem/ethnicity",
            code: "2",
            display: "Malay"
          }
        ]
      }
    },
    {
      url: "https://fhir.synapxe.sg/StructureDefinition/ext-residential-status",
      valueCodeableConcept: {
        coding: [
          {
            system: "https://fhir.synapxe.sg/CodeSystem/residential-status",
            code: "P",
            display: "Singapore PR"
          }
        ]
      }
    },
    {
      url: "https://fhir.synapxe.sg/StructureDefinition/ext-subsidy",
      valueCodeableConcept: {
        coding: [
          {
            system: "https://fhir.synapxe.sg/CodeSystem/subsidy",
            code: "MG",
            display: "Merdeka Generation"
          }
        ]
      }
    },
    {
      url: "https://fhir.synapxe.sg/StructureDefinition/ext-consent-for-sharing",
      valueBoolean: true
    },
    {
      url: "https://fhir.synapxe.sg/StructureDefinition/ext-grc",
      valueCodeableConcept: {
        coding: [
          {
            system: "https://fhir.synapxe.sg/CodeSystem/grc",
            code: "GRC001",
            display: "Tampines GRC"
          }
        ]
      }
    },
    {
      url: "https://fhir.synapxe.sg/StructureDefinition/ext-constituency",
      valueCodeableConcept: {
        coding: [
          {
            system: "https://fhir.synapxe.sg/CodeSystem/constituency",
            code: "CONST001",
            display: "Tampines"
          }
        ]
      }
    }
  ]
});

const generateEncounterResource = () => ({
  resourceType: "Encounter",
  id: "encounter-001",
  status: "completed",
  identifier: [
    {
      system: "https://fhir.synapxe.sg/NamingSystem/event-id",
      value: `EVT-${new Date().toISOString().slice(0, 10).replace(/-/g, '')}-${Math.floor(Math.random() * 10000)}`
    }
  ],
  period: {
    start: new Date().toISOString(),
    end: new Date(Date.now() + 1800000).toISOString() // 30 minutes later
  }
});

const generateHealthcareServiceResource = () => ({
  resourceType: "HealthcareService",
  id: "healthcareservice-001",
  active: true,
  identifier: [
    {
      system: "https://fhir.synapxe.sg/NamingSystem/screening-campaign",
      value: `SC-${new Date().getFullYear()}-${String(Math.floor(Math.random() * 1000)).padStart(3, '0')}`
    }
  ],
  name: "Mobile Screening Bus"
});

const generateLocationResource = () => ({
  resourceType: "Location",
  id: "location-001",
  name: "Bedok Community Centre",
  address: {
    line: ["Bedok Community Centre"],
    postalCode: "469000"
  }
});

const generateOrganizationResource = (type) => {
  const isProvider = type === "provider";
  return {
    resourceType: "Organization",
    id: isProvider ? "organization-provider-001" : "organization-cluster-001",
    type: [
      {
        coding: [
          {
            system: "https://fhir.synapxe.sg/CodeSystem/organization-type",
            code: isProvider ? "PROVIDER" : "CLUSTER",
            display: isProvider ? "Provider Organization" : "Cluster Organization"
          }
        ]
      }
    ],
    identifier: [
      {
        system: "https://fhir.synapxe.sg/NamingSystem/hci-code",
        value: isProvider ? "HCI003" : "CL003"
      }
    ],
    name: isProvider ? "Raffles Medical" : "SingHealth"
  };
};

const generateObservationResource = (screeningType, questions, metadata) => {
  const screeningTypeDisplay = {
    HS: "Hearing Screening",
    OS: "Oral Screening",
    VS: "Vision Screening"
  };

  const components = questions.slice(0, 2).map(question => {
    // Pick first allowed answer or random one
    const answer = question.AllowedAnswers[Math.floor(Math.random() * question.AllowedAnswers.length)];
    
    return {
      code: {
        coding: [
          {
            system: "https://fhir.synapxe.sg/CodeSystem/screening-questionnaire",
            code: question.QuestionCode,
            display: question.QuestionDisplay
          }
        ]
      },
      valueString: answer
    };
  });

  return {
    resourceType: "Observation",
    id: `observation-${screeningType.toLowerCase()}-001`,
    status: "final",
    code: {
      coding: [
        {
          system: "https://fhir.synapxe.sg/CodeSystem/screening-type",
          code: screeningType,
          display: screeningTypeDisplay[screeningType]
        }
      ]
    },
    component: components
  };
};

const getMinimalSampleBundle = () => ({
  resourceType: "Bundle",
  type: "collection",
  entry: [
    {
      resource: generatePatientResource()
    },
    {
      resource: generateEncounterResource()
    },
    {
      resource: generateHealthcareServiceResource()
    },
    {
      resource: generateLocationResource()
    },
    {
      resource: generateOrganizationResource("provider")
    },
    {
      resource: generateOrganizationResource("cluster")
    }
  ]
});
