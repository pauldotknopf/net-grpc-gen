#include "qtest1.h"
#include "proto/gen.grpc.pb.h"
#include <grpc++/grpc++.h>
#include <QDebug>

class QTest1Private
{
public:
    QTest1Private() : objectId(0) {

    }
    std::unique_ptr<Tests::Test1ObjectService::Stub> test1ObjectService;
    grpc::ClientContext objectRequestContext;
    std::unique_ptr<grpc::ClientReaderWriter<google::protobuf::Any, google::protobuf::Any>> objectRequest;
    google::protobuf::uint64 objectId;

    void createObject() {
        objectRequest = test1ObjectService->Create(&objectRequestContext);
        google::protobuf::Any createResponseAny;
        if(!objectRequest->Read(&createResponseAny)) {
            qCritical("Failed to read request from object creation.");
            objectRequest.release();
            return;
        }

        Tests::Test1CreateResponse createResponse;
        if(!createResponseAny.UnpackTo(&createResponse)) {
            qCritical("Couldn't unpack request to CreateResponse.");
            objectRequest.release();
            return;
        }

        objectId = createResponse.objectid();
    }

    void releaseObject() {
        if(objectRequest != nullptr) {
            auto result = objectRequest->Finish();
            if(!result.ok()) {
                qCritical("Couldn't dispose of the object: %s", result.error_message().c_str());
            }
        }
    }
};

QTest1::QTest1() : d_ptr(new QTest1Private())
{
    auto channel = grpc::CreateChannel("localhost:8000", grpc::InsecureChannelCredentials());
    auto stub = Tests::Test1ObjectService::NewStub(channel);
    d_ptr->test1ObjectService = std::move(stub);

    d_ptr->createObject();
}

QTest1::~QTest1()
{
    d_ptr->releaseObject();
}

QString QTest1::getPropString()
{
    grpc::ClientContext context;
    Tests::Test1PropStringGetRequest request;
    request.set_objectid(d_ptr->objectId);
    Tests::Test1PropStringGetResponse response;
    auto result = d_ptr->test1ObjectService->GetPropertyPropString(&context, request, &response);
    if(!result.ok()) {
        qCritical("Couldn't get the property: %s", result.error_message().c_str());
        return QString();
    }
    auto val = response.value();
    return QString::fromStdString(response.value().value());
}

void QTest1::setPropString(QString& val)
{
    grpc::ClientContext context;
    Tests::Test1PropStringSetRequest request;
    request.set_objectid(d_ptr->objectId);
    auto str = new google::protobuf::StringValue();
    str->set_value(val.toStdString());
    request.set_allocated_value(str);
    Tests::Test1PropStringSetResponse response;
    auto result = d_ptr->test1ObjectService->SetPropertyPropString(&context, request, &response);
    if(!result.ok()) {
        qCritical("Couldn't get the property: %s", result.error_message().c_str());
        return;
    }
}
