# 🛍️ eShop Evolution

Repositório demonstrativo da evolução arquitetural de uma aplicação e-commerce. O sistema evoluiu de um monólito para uma **Arquitetura de Microservices**, composta por **3 APIs independentes**.

---

## 🏗️ Nova Arquitetura: Microservices

A aplicação foi refatorada e segregada em quatro serviços autônomos, cada um com sua responsabilidade bem definida:

- **Catalog API**: Gerenciamento e consulta do catálogo de produtos.
- **Basket API**: Gestão do carrinho de compras e itens do usuário.
- **Discount API**: Aplicação de descontos e cupons.
- **Ordering API**: Processamento e efetivação de pedidos.

Essa separação permite escalar cada serviço independentemente (Horizontal Scaling) e isolar falhas.

## 🤝 RaptorMediator => Padrão Mediator

Para orquestrar as intenções de negócio dentro de cada serviço, adotamos o **Padrão Mediator**.

O Mediator desacopla o recebimento da requisição (geralmente no Controller) de sua execução (Domain Handler).
- **Resumo:** Em vez de `Controller -> Service Class`, temos `Controller -> Mediator -> Handler`.
- **Vantagem:** Facilita a implementação de *Cross-Cutting Concerns* (Logs, Validações, Transações, Retries) através de Pipelines, sem sujar a regra de negócio.

## 🚀 gRPC: Alta Performance entre Serviços

Utilizamos **gRPC** para a comunicação síncrona crítica entre a **Basket API** e a **Discount API**, garantindo validação de descontos em tempo real com mínima latência.

- **Performance Superior:** Baseado em **HTTP/2** e **Protocol Buffers**, oferece performance até 10x superior a REST/JSON.
- **Contrato-First:** A definição da API via arquivos `.proto` assegura interoperabilidade e tipagem forte.
- **Arquitetura do Fluxo:**
  - **Discount API (Server):** Microserviço autônomo que detém as regras de negócio de descontos.
  - **Basket API (Client):** Consome o serviço de desconto transparentemente através de um cliente gRPC gerado.
- **Detalhes Técnicos e de Negócio:**
  - **HTTP/2:** Permite multiplexação de requisições sobre uma única conexão TCP, reduzindo overhead.
  - **Protocol Buffers:** Formato de serialização binário eficiente, menor que JSON/XML, otimizado para velocidade.
  - **Streaming Bidirecional:** Capacidade de enviar e receber múltiplos dados em uma única chamada, ideal para cenários de tempo real.
  - **Validação de Descontos:** A Basket API envia o carrinho para a Discount API via gRPC, que retorna os descontos aplicáveis, tudo em milissegundos.

*Saimos de uma arquitura tier para uma arquitura modular e agora atingimos a maturidade em microservices.*

![Versão Final](images/bigPictureApp.png)