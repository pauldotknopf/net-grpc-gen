#include "qtest2worker.h"
#include "gen.pb.h"
#include "gen.grpc.pb.h"
#include "roc-lib/qrocobjectadapter.h"
#include "protobuf-qjson/protobufjsonconverter.h"
using namespace Tests;
class Tests::QTest2WorkerPrivate
{
public:
	QTest2WorkerPrivate() : objectId(0) {}
	google::protobuf::uint64 objectId;
	std::unique_ptr<Tests::Test2ObjectService::Stub> service;
	grpc::ClientContext objectRequestContext;
	std::unique_ptr<grpc::ClientReaderWriter<google::protobuf::Any, google::protobuf::Any>> objectRequest;
	
	void createObject()
	{
		objectRequest = service->Create(&objectRequestContext);
		google::protobuf::Any createResponseAny;
		if(!objectRequest->Read(&createResponseAny))
		{
			qCritical("Failed to read request from object creation.");
			objectRequest.release();
			return;
		}
		Tests::Test2CreateResponse createResponse;
		if(!createResponseAny.UnpackTo(&createResponse))
		{
			qCritical("Failed to unpack request from object creation.");
			objectRequest.release();
			return;
		}
		objectId = createResponse.objectid();
	}
	void releaseObject()
	{
		if(objectRequest != nullptr)
		{
			auto result = objectRequest->Finish();
			if(!result.ok()) { qCritical("Couldn't dispose of object: %s", result.error_message().c_str()); }
		}
	}
	bool isValid()
	{
		return objectRequest != nullptr;
	}
};
QTest2Worker::QTest2Worker() : QObject(nullptr), d_priv(new QTest2WorkerPrivate())
{
	auto channel = QRocObjectAdapter::getSharedChannel();
	if(channel == nullptr) { qWarning("Set the channel to use via QRocObjectAdapter::setSharedChannel(...)"); return; }
	d_priv->service = Tests::Test2ObjectService::NewStub(channel);
	d_priv->createObject();
}
QTest2Worker::~QTest2Worker()
{
	d_priv->releaseObject();
}
void QTest2Worker::testMethod2(QJsonValue val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test2TestMethod2MethodRequest request;
		Tests::Test2TestMethod2MethodResponse response;
		request.set_objectid(d_priv->objectId);
		auto messageVal = new custom::types::TestMessageRequest();
		ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
		request.set_allocated_value(messageVal);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethod2(&context, request, &response);
		auto responseValue = response.value();
		QJsonValue messageJson;
		ProtobufJsonConverter::messageToJsonValue(&responseValue, messageJson);
		if(!invokeResult.ok()) {
			emit testMethod2Done(messageJson, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethod2Done(messageJson, requestId, QString());
		}
	});
}
void QTest2Worker::testMethodSync2(QJsonValue val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test2TestMethodSync2MethodRequest request;
		Tests::Test2TestMethodSync2MethodResponse response;
		request.set_objectid(d_priv->objectId);
		auto messageVal = new custom::types::TestMessageRequest();
		ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
		request.set_allocated_value(messageVal);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodSync2(&context, request, &response);
		auto responseValue = response.value();
		QJsonValue messageJson;
		ProtobufJsonConverter::messageToJsonValue(&responseValue, messageJson);
		if(!invokeResult.ok()) {
			emit testMethodSync2Done(messageJson, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodSync2Done(messageJson, requestId, QString());
		}
	});
}
void QTest2Worker::testMethodWithNoResponse2(QJsonValue val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test2TestMethodWithNoResponse2MethodRequest request;
		Tests::Test2TestMethodWithNoResponse2MethodResponse response;
		request.set_objectid(d_priv->objectId);
		auto messageVal = new custom::types::TestMessageRequest();
		ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
		request.set_allocated_value(messageVal);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodWithNoResponse2(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testMethodWithNoResponse2Done(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodWithNoResponse2Done(requestId, QString());
		}
	});
}
void QTest2Worker::testMethodNoRequest2(int requestId)
{
	QMetaObject::invokeMethod(this, [this, requestId] {
		Tests::Test2TestMethodNoRequest2MethodRequest request;
		Tests::Test2TestMethodNoRequest2MethodResponse response;
		request.set_objectid(d_priv->objectId);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodNoRequest2(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testMethodNoRequest2Done(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodNoRequest2Done(requestId, QString());
		}
	});
}
