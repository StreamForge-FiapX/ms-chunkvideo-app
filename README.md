![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=POSTECH-SOAT-SALA11_application-avalanches-pagamento-ms&metric=alert_status)
![Bugs](https://sonarcloud.io/api/project_badges/measure?project=POSTECH-SOAT-SALA11_application-avalanches-pedido-ms&metric=bugs)
![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=POSTECH-SOAT-SALA11_application-avalanches-pedido-ms&metric=code_smells)
![Coverage](https://sonarcloud.io/api/project_badges/measure?project=POSTECH-SOAT-SALA11_application-avalanches-pagamento-ms&metric=coverage)
# Documentação do Microserviço de Processamento de Vídeos: ms-chunkvideo-app

Este documento descreve o fluxo de funcionamento do microserviço responsável pelo processamento inicial de vídeos enviados para o sistema. Ele aborda as etapas necessárias para o desenvolvimento, incluindo integrações, papéis no sistema e detalhamento técnico das ações esperadas.

---

## Visão Geral do Sistema

O sistema é um gerador de frames a partir de vídeos enviados pelos usuários. Após o upload de um vídeo no portal, o sistema processa o arquivo e retorna um `.zip` contendo imagens extraídas a cada segundo do vídeo. 

Este microserviço desempenha um papel fundamental na etapa inicial de processamento, dividindo os vídeos em pequenos pedaços (chunks) de até 1 minuto cada, que serão utilizados em fluxos subsequentes.

---

## Objetivo do Microserviço

O microserviço é responsável por:

1. Consumir mensagens de uma fila no **RabbitMQ** para identificar quando um novo vídeo foi enviado ao sistema.
2. Fazer o download do vídeo de um **Bucket S3**.
3. Dividir o vídeo em pequenos pedaços (chunks) de até 1 minuto cada.
4. Realizar o upload de cada chunk gerado para outro **Bucket S3**.
5. Atualizar o banco de dados com informações dos chunks gerados.
6. Publicar mensagens no **RabbitMQ** informando que cada chunk foi gerado e armazenado.

---

## Fluxo de Funcionamento

1. **Recepção de Mensagem**
   - O microserviço se conecta a uma fila no RabbitMQ chamada 'queue-uploaded-video' (dentro do mesmo cluster Kubernetes em execução no EKS da AWS).
   - Ele consome mensagens que indicam que um vídeo foi enviado para o Bucket S3 'uploaded-video-bucket'.

2. **Processamento de Vídeo**
   - Após receber a mensagem, o microserviço:
     - Faz o download do vídeo do Bucket S3 'uploaded-video-bucket'.
     - Divide o vídeo em chunks de até 1 minuto cada.

3. **Ações para Cada Chunk**
   - Para cada chunk gerado:
     - Faz o upload do chunk para o Bucket S3 'uploaded-video-chunk-bucket'.
     - Atualiza o banco de dados 'chunkvideo-psql-db'(**PostgreSQL**, hospedado no RDS) com informações como:
       - Identificador do vídeo original.
       - Identificadores dos chunks gerados.
       - Quantidade de chunks.
       - Destinos dos chunks no Bucket S3.
     - Publica uma mensagem na fila RabbitMQ 'chunk-video-process' informando que o chunk foi processado e está disponível.

---

## Integrações e Dependências

### 1. **RabbitMQ**
   - Consumir mensagens da fila indicando novos vídeos para processamento.
   - Publicar mensagens na fila indicando que os chunks foram gerados.

### 2. **Amazon S3**
   - Download do vídeo original a partir de um Bucket específico.
   - Upload dos chunks gerados para outro Bucket.

### 3. **Banco de Dados (PostgreSQL)**
   - Registro das informações sobre os chunks gerados:
     - Quantidade.
     - Identificadores únicos.
     - Destinos.

---

## Tecnologias e Ferramentas Utilizadas

- **Linguagem e Framework**: .NET Core (C#)
- **Orquestração de Contêineres**: Kubernetes (AWS EKS)
- **Banco de Dados**: PostgreSQL (AWS RDS)
- **Mensageria**: RabbitMQ
- **Armazenamento**: AWS S3

---

## Regras de Negócio e Pontos Críticos

1. **Durabilidade dos Chunks**
   - Cada chunk deve ter uma duração máxima de 1 minuto.
   - Em casos de vídeos menores, o chunk será único e terá o tamanho do vídeo completo.

2. **Ordem das Operações**
   - Garantir que o upload no S3, a gravação no banco de dados e o envio de mensagens ao RabbitMQ sejam executados de forma síncrona ou com estratégias de rollback em caso de falhas.

3. **Mensagens no RabbitMQ**
   - As mensagens enviadas para o RabbitMQ devem conter:
     - Identificador do chunk.
     - Localização no S3.
     - Informações relevantes para o próximo serviço no pipeline.

4. **Gerenciamento de Erros**
   - Implementar tolerância a falhas para operações no S3, RabbitMQ e banco de dados.
   - Retentar operações falhas com lógica de backoff exponencial.

5. **Escalabilidade**
   - O microserviço deve ser escalável horizontalmente, suportando um alto volume de mensagens e processamento simultâneo de vídeos.

---

## Estrutura do Projeto

O Projeto deverá seguir a implementação arquitetural no padrão Hexagonal.

Aqui está uma estrutura básica para implementar a **Arquitetura Hexagonal** no microserviço **ms-chunkvideo-app**:

---

### **Estrutura de Diretórios**

```
ms-chunkvideo-app/
├── src/
│   ├── application/
│   │   ├── ports/
│   │   │   ├── VideoProcessorPort.cs
│   │   │   ├── MessagePublisherPort.cs
│   │   │   └── StoragePort.cs
│   │   └── usecases/
│   │       ├── ProcessVideoUseCase.cs
│   │       └── UpdateChunkMetadataUseCase.cs
│   ├── domain/
│   │   ├── entities/
│   │   │   ├── Video.cs
│   │   │   └── Chunk.cs
│   │   ├── exceptions/
│   │   │   └── VideoProcessingException.cs
│   │   └── services/
│   │       └── VideoChunkingService.cs
│   ├── infrastructure/
│   │   ├── adapters/
│   │   │   ├── RabbitMqPublisherAdapter.cs
│   │   │   ├── S3StorageAdapter.cs
│   │   │   └── PostgresDatabaseAdapter.cs
│   │   ├── configuration/
│   │   │   ├── DependencyInjectionConfig.cs
│   │   │   └── AppSettings.json
│   │   └── framework/
│   │       └── KubernetesWorkerService.cs
│   ├── api/
│   │   └── VideoProcessingController.cs
│   └── Program.cs
├── tests/
│   ├── unit/
│   │   ├── VideoChunkingServiceTests.cs
│   │   └── ProcessVideoUseCaseTests.cs
│   ├── integration/
│   │   ├── RabbitMqIntegrationTests.cs
│   │   └── S3StorageIntegrationTests.cs
│   └── e2e/
│       └── FullProcessingFlowTests.cs
├── kubernetes/
│   ├── deployment.yaml
│   ├── service.yaml
│   ├── hpa.yaml
└── README.md
```

---

### **Principais Componentes**

#### **1. `domain` (Domínio)**
- Contém a lógica central do negócio, independente de frameworks ou dependências externas.
- **Entidades (`entities/`)**:
  - Representam objetos fundamentais no domínio, como `Video` e `Chunk`.
- **Serviços (`services/`)**:
  - Contêm a lógica de processamento, como dividir vídeos em chunks (ex.: `VideoChunkingService`).
- **Exceções (`exceptions/`)**:
  - Definem erros específicos do domínio, como `VideoProcessingException`.

---

#### **2. `application` (Aplicação)**
- Contém os casos de uso e as portas.
- **Portas (`ports/`)**:
  - Interfaces que o domínio usa para interagir com o mundo externo (RabbitMQ, S3, banco de dados).
  - Exemplos:
    - `VideoProcessorPort`: Define as operações de processamento de vídeo.
    - `MessagePublisherPort`: Abstrai o envio de mensagens ao RabbitMQ.
    - `StoragePort`: Define operações de upload/download para o S3.
- **Casos de Uso (`usecases/`)**:
  - Implementam os fluxos de trabalho principais.
  - Exemplos:
    - `ProcessVideoUseCase`: Coordena o download do vídeo, chunking e upload dos chunks.
    - `UpdateChunkMetadataUseCase`: Atualiza o banco de dados com informações dos chunks.

---

#### **3. `infrastructure` (Infraestrutura)**
- Implementa as dependências externas, conectando-se às portas definidas na camada de aplicação.
- **Adaptadores (`adapters/`)**:
  - Implementam as interfaces definidas nas portas.
  - Exemplo:
    - `RabbitMqPublisherAdapter`: Implementa `MessagePublisherPort` para comunicação com RabbitMQ.
    - `S3StorageAdapter`: Implementa `StoragePort` para interagir com o S3.
    - `PostgresDatabaseAdapter`: Implementa operações com o PostgreSQL.
- **Configuração (`configuration/`)**:
  - Gerencia dependências, injeções e configurações (ex.: `DependencyInjectionConfig`).
- **Framework (`framework/`)**:
  - Gerencia interações específicas de execução, como a inicialização do worker no Kubernetes.

---

#### **4. `api` (API)**
- Camada de exposição que recebe requisições externas (caso necessário) e inicia os fluxos de trabalho.
- **Controladores (`VideoProcessingController`)**:
  - Exemplo: Um endpoint HTTP que aciona o fluxo de processamento para debugging ou monitoramento.
