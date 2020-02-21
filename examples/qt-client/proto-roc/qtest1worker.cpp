#include "qtest1worker.h"
#include "gen.pb.h"
#include "gen.grpc.pb.h"
#include "roc-lib/qrocobjectadapter.h"
#include "protobuf-qjson/protobufjsonconverter.h"
using namespace Tests;
class Tests::QTest1WorkerPrivate
{
public:
	QTest1WorkerPrivate() : objectId(0) {}
	google::protobuf::uint64 objectId;
	std::unique_ptr<Tests::Test1ObjectService::Stub> service;
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
		Tests::Test1CreateResponse createResponse;
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
QTest1Worker::QTest1Worker() : QObject(nullptr), d_priv(new QTest1WorkerPrivate())
{
	auto channel = QRocObjectAdapter::getSharedChannel();
	if(channel == nullptr) { qWarning("Set the channel to use via QRocObjectAdapter::setSharedChannel(...)"); return; }
	d_priv->service = Tests::Test1ObjectService::NewStub(channel);
	d_priv->createObject();
}
QTest1Worker::~QTest1Worker()
{
	d_priv->releaseObject();
}
void QTest1Worker::testMethod(QJsonValue val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test1TestMethodMethodRequest request;
		Tests::Test1TestMethodMethodResponse response;
		request.set_objectid(d_priv->objectId);
		auto messageVal = new custom::types::TestMessageRequest();
		ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
		request.set_allocated_value(messageVal);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethod(&context, request, &response);
		auto responseValue = response.value();
		QJsonValue messageJson;
		ProtobufJsonConverter::messageToJsonValue(&responseValue, messageJson);
		if(!invokeResult.ok()) {
			emit testMethodDone(messageJson, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodDone(messageJson, requestId, QString());
		}
	});
}
void QTest1Worker::testMethodSync(QJsonValue val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test1TestMethodSyncMethodRequest request;
		Tests::Test1TestMethodSyncMethodResponse response;
		request.set_objectid(d_priv->objectId);
		auto messageVal = new custom::types::TestMessageRequest();
		ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
		request.set_allocated_value(messageVal);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodSync(&context, request, &response);
		auto responseValue = response.value();
		QJsonValue messageJson;
		ProtobufJsonConverter::messageToJsonValue(&responseValue, messageJson);
		if(!invokeResult.ok()) {
			emit testMethodSyncDone(messageJson, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodSyncDone(messageJson, requestId, QString());
		}
	});
}
void QTest1Worker::testMethodWithNoResponse(QJsonValue val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test1TestMethodWithNoResponseMethodRequest request;
		Tests::Test1TestMethodWithNoResponseMethodResponse response;
		request.set_objectid(d_priv->objectId);
		auto messageVal = new custom::types::TestMessageRequest();
		ProtobufJsonConverter::jsonValueToMessage(val, messageVal);
		request.set_allocated_value(messageVal);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodWithNoResponse(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testMethodWithNoResponseDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodWithNoResponseDone(requestId, QString());
		}
	});
}
void QTest1Worker::testMethodPrimitive(int val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test1TestMethodPrimitiveMethodRequest request;
		Tests::Test1TestMethodPrimitiveMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodPrimitive(&context, request, &response);
		auto responseValue = response.value();
		if(!invokeResult.ok()) {
			emit testMethodPrimitiveDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodPrimitiveDone(responseValue, requestId, QString());
		}
	});
}
void QTest1Worker::testMethodNoRequestOrResponse(int requestId)
{
	QMetaObject::invokeMethod(this, [this, requestId] {
		Tests::Test1TestMethodNoRequestOrResponseMethodRequest request;
		Tests::Test1TestMethodNoRequestOrResponseMethodResponse response;
		request.set_objectid(d_priv->objectId);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodNoRequestOrResponse(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testMethodNoRequestOrResponseDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodNoRequestOrResponseDone(requestId, QString());
		}
	});
}
void QTest1Worker::testMethodNoRequest(int requestId)
{
	QMetaObject::invokeMethod(this, [this, requestId] {
		Tests::Test1TestMethodNoRequestMethodRequest request;
		Tests::Test1TestMethodNoRequestMethodResponse response;
		request.set_objectid(d_priv->objectId);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodNoRequest(&context, request, &response);
		auto responseValue = response.value();
		if(!invokeResult.ok()) {
			emit testMethodNoRequestDone(responseValue, requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodNoRequestDone(responseValue, requestId, QString());
		}
	});
}
void QTest1Worker::testMethodNoResponse(int val, int requestId)
{
	QMetaObject::invokeMethod(this, [this, val, requestId] {
		Tests::Test1TestMethodNoResponseMethodRequest request;
		Tests::Test1TestMethodNoResponseMethodResponse response;
		request.set_objectid(d_priv->objectId);
		request.set_value(val);
		grpc::ClientContext context;
		auto invokeResult = d_priv->service->InvokeTestMethodNoResponse(&context, request, &response);
		if(!invokeResult.ok()) {
			emit testMethodNoResponseDone(requestId, QString::fromStdString(invokeResult.error_message()));
		} else {
			emit testMethodNoResponseDone(requestId, QString());
		}
	});
}
